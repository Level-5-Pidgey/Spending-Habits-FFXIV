using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Business;
using Core.Enum;
using Core.Model;
using Dalamud.DrunkenToad;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Localization = Dalamud.Localization;

namespace Infrastructure
{
    public class GoodsOrServiceTracker : BaseRepository
    {
        private DalamudPluginInterface PluginInterface { get; }
        private ChatGui GameChat { get; }
        private SpendingHabitsFileManager FileManager { get; }

        private int _lastTeleportFee = 0;
        private readonly object _transactLocker = new ();
        private const string TeleportId = "TeleportFee";

        public GoodsOrServiceTracker(
            SpendingHabitsFileManager fileManager,
            DalamudPluginInterface pluginInterface, 
            ChatGui gameChat) : base(pluginInterface.GetPluginConfigDirectory())
        {
            //Assign properties
            PluginInterface = pluginInterface;
            GameChat = gameChat;
            FileManager = fileManager;

            //Assign events
            gameChat.ChatMessage += TrackMessage;
        }
        
        private void TrackMessage(XivChatType chatType, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!FileManager.Config.Active) return;

            var chatText = message.TextValue;
            Logger.LogVerbose($"[{Enum.GetName(chatType)} ({(int) chatType})] {chatText}");

            switch ((RelevantChatType) chatType)
            {
                case RelevantChatType.CharacterInternalMessage:
                    if (chatText.StartsWith("You spent"))
                    {
                        if (chatText.EndsWith("gil."))
                        {
                            _lastTeleportFee = ParseNumberFromString(chatText);
                        }
                        else
                        {
                            Logger.LogDebug($"Player spent currency other than gil [{chatText}]");
                        }
                    }
                    
                    break;
                case RelevantChatType.CharacterAction:
                    if (chatText.StartsWith("You use Teleport"))
                    {
                        if (_lastTeleportFee > -1)
                        {
                            //If they've just completed a teleport and paid the fee, save this value to DB
                            UpdateGoodsOrServiceTransaction(TeleportId, CurrencyType.Gil, _lastTeleportFee);

                            //Now that we've saved the teleport fee we can reset the value again.
                            _lastTeleportFee = 0;
                        }
                    
                        //Overwrite teleport text to tell user how much they've spent on teleports
                        UpdateTeleportText(chatType, sender, message);
                    }
                    
                    break;
                case RelevantChatType.CurrencyObtained:
                    //Obtained from quests
                    break;
            }
        }

        private void UpdateTeleportText(XivChatType chatType, SeString sender, SeString messageString)
        {
            var teleportService = GetItem<GoodsOrService>(x => x.Name == TeleportId);
            var teleportCosts = 0;
            if (teleportService is not null)
            {
                teleportCosts = teleportService.CurrencyTransactions[CurrencyType.Gil].TotalExpenses;
            }
            
            foreach (var messagePayload in messageString.Payloads)
            {
                if (messagePayload is TextPayload textPayload)
                {
                    textPayload.Text += $" You have spent {teleportCosts} Gil on teleports so far.";
                };
            }
            
            GameChat.Print(messageString);
        }
        
        public void Dispose()
        {
            GameChat.ChatMessage -= TrackMessage;
        }

        public IEnumerable<GoodsOrService> GetAll()
        {
            return GetItems<GoodsOrService>();
        }

        private void UpdateGoodsOrServiceTransaction(string goodsOrServiceName, CurrencyType currencyType, int transactionValue, int count = 1)
        {
            if (transactionValue == 0) return;
            
            lock (_transactLocker)
            {
                //Retrieve good or service from DB
                var existingItem = GetItem<GoodsOrService>(x => x.Name == goodsOrServiceName);

                //Update fields or create new item
                if (existingItem is not null)
                {
                    existingItem.Updated = DateTime.Now;

                    var currencyTransaction = existingItem.CurrencyTransactions.ContainsKey(currencyType)
                        ? existingItem.CurrencyTransactions[currencyType]
                        : new CurrencyTransaction();

                    //Edit transactions for the item
                    UpdateTransactionForType(transactionValue, count, currencyTransaction);

                    UpdateItem(existingItem);
                }
                else
                {
                    var newItem = new GoodsOrService
                    {
                        Name = goodsOrServiceName,
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        CurrencyTransactions = new Dictionary<CurrencyType, CurrencyTransaction>()
                        {
                            {
                                currencyType, CreateTransactionForType(transactionValue, count)
                            }
                        }
                    };
                    
                    InsertItem(newItem);
                }
            }
        }

        private static void UpdateTransactionForType(int transactionValue, int count,
            CurrencyTransaction currencyTransaction)
        {
            switch (transactionValue)
            {
                case > 0:
                    currencyTransaction.LastBought = DateTime.Now;
                    currencyTransaction.QuantityBought += count;
                    currencyTransaction.TotalExpenses += transactionValue;
                    break;
                case < 0:
                    currencyTransaction.LastSold = DateTime.Now;
                    currencyTransaction.QuantitySold += count;
                    currencyTransaction.TotalProfit += transactionValue;
                    break;
            }
        }
        
        private static CurrencyTransaction CreateTransactionForType(int transactionValue, int count)
        {
            var result = new CurrencyTransaction();
            
            switch (transactionValue)
            {
                case > 0:
                    result.LastBought = DateTime.Now;
                    result.QuantityBought += count;
                    result.TotalExpenses += transactionValue;
                    break;
                case < 0:
                    result.LastSold = DateTime.Now;
                    result.QuantitySold += count;
                    result.TotalProfit += transactionValue;
                    break;
            }

            return result;
        }

        private int ParseNumberFromString(string gameMessage) => int.Parse(new string(gameMessage.Where(char.IsDigit).ToArray()));
    }
}