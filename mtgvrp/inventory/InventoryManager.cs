using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.core.Items;
using mtgvrp.inventory.bags;
using mtgvrp.job_manager.fisher;
using mtgvrp.phone_manager;
using mtgvrp.player_manager;
using mtgvrp.property_system.businesses;
using mtgvrp.weapon_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace mtgvrp.inventory
{
    class InventoryManager : Script
    {
        public InventoryManager()
        {
            _activeInvsBeingManaged = new Dictionary<Client, KeyValuePair<IStorage, IStorage>>();

            #region Inventory Items

            BsonClassMap.RegisterClassMap<BagItem>();
            BsonClassMap.RegisterClassMap<Phone>();
            BsonClassMap.RegisterClassMap<Money>();
            BsonClassMap.RegisterClassMap<Fish>();
            BsonClassMap.RegisterClassMap<RopeItem>();
            BsonClassMap.RegisterClassMap<RagsItem>();
            BsonClassMap.RegisterClassMap<SprunkItem>();
            BsonClassMap.RegisterClassMap<CheckItem>();

            BsonClassMap.RegisterClassMap<Weapon>();
            #endregion

            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        #region Events

        public class OnGetItemEventArgs : EventArgs
        {
            public IInventoryItem Item { get; private set; }
            public int Amount { get; private set; }
            public OnGetItemEventArgs(IInventoryItem item, int amount)
            {
                Item = item;
                Amount = amount;
            }
        }
        public delegate void StorageGetItemHandler(IStorage sender, OnGetItemEventArgs args);
        public static event StorageGetItemHandler OnStorageGetItem;

        public class OnLoseItemEventArgs : EventArgs
        {
            public IInventoryItem Item { get; private set; }
            public int Amount { get; private set; }
            public OnLoseItemEventArgs(IInventoryItem item, int amount)
            {
                Item = item;
                Amount = amount;
            }
        }
        public delegate void StorageLoseItemHandler(IStorage sender, OnLoseItemEventArgs args);
        public static event StorageLoseItemHandler OnStorageLoseItem;

        public class OnItemAmountUpdatedEventArgs : EventArgs
        {
            public Type Item { get; private set; }
            public int Amount { get; private set; }
            public OnItemAmountUpdatedEventArgs(Type item, int amount)
            {
                Item = item;
                Amount = amount;
            }
        }
        public delegate void OnItemAmountUpdatedEventHandler(IStorage sender, OnItemAmountUpdatedEventArgs args);
        public static event OnItemAmountUpdatedEventHandler OnStorageItemUpdateAmount;
        #endregion

        public enum GiveItemErrors
        {
            NotEnoughSpace,
            HasBlockingItem,
            MaxAmountReached,
            Success,
            HasSimilarItem
        }
        private static IInventoryItem CloneItem(IInventoryItem item, int amount = -1)
        {
            var type = item.GetType();
            var properties = type.GetProperties();
            var newObject = ItemTypeToNewObject(type);
            foreach (var prop in properties)
            {
                if(prop.CanWrite)
                    prop.SetValue(newObject, prop.GetValue(item));
            }
            if (amount != -1) newObject.Amount = amount;
            return newObject;
        }


        /// <summary>
        /// Gives player an item.
        /// NOTE: The item object is cloned, so the actual object passed isn't referenced.
        /// Use DeleteInventoryItem to take.
        /// </summary>
        /// <param name="storage">The storage you wanna add to.</param>
        /// <param name="sentitem">The item object, the Amount inside is ignored.</param>
        /// <param name="amount">The amount of this item to be added, item will be duplicated if not stackable.</param>
        /// <param name="ignoreBlocking">Add even if inventory is blocked due to big item.</param>
        /// <returns></returns>
        public static GiveItemErrors GiveInventoryItem(IStorage storage, IInventoryItem sentitem, int amount = 1, bool ignoreBlocking = false)
        {
            //We wanna clone and add it.
            var item = CloneItem(sentitem, amount);

            if (storage.Inventory == null) storage.Inventory = new List<IInventoryItem>();
            //Make sure he doesn't have blocking item.
            if(storage.Inventory.FirstOrDefault(x => x.IsBlocking == true) != null && ignoreBlocking == false)
                return GiveItemErrors.HasBlockingItem;

            int maxAmount = -1;
            foreach (var amnt in item.MaxAmount)
            {
                if (amnt.Key == storage.GetType())
                {
                    maxAmount = amnt.Value;
                }
            }

            //Check if player has simliar item.
            var oldItem = storage.Inventory.FirstOrDefault(x => x.CommandFriendlyName == item.CommandFriendlyName);
            if (oldItem == null || oldItem.CanBeStacked == false)
            {
                if (maxAmount != -1 && oldItem?.Amount >= maxAmount)
                {
                    return GiveItemErrors.MaxAmountReached;
                }

                if (oldItem?.CommandFriendlyName == sentitem.CommandFriendlyName)
                {
                    return GiveItemErrors.HasSimilarItem;
                }
                //Check if has enough space.
                if ((GetInventoryFilledSlots(storage) + item.Amount * item.AmountOfSlots) <= storage.MaxInvStorage)
                {
                    //Set an id.
                    if(item.Id == ObjectId.Empty) { item.Id = ObjectId.GenerateNewId(DateTime.Now); sentitem.Id = item.Id; }

                    //Add.
                    storage.Inventory.Add(item);
                    OnStorageGetItem?.Invoke(storage, new OnGetItemEventArgs(sentitem, amount));
                    OnStorageItemUpdateAmount?.Invoke(storage, new OnItemAmountUpdatedEventArgs(item.GetType(), item.Amount));
                    return GiveItemErrors.Success;
                }
                else
                    return GiveItemErrors.NotEnoughSpace;
            }
            else
            {

                if (maxAmount != -1 && oldItem.Amount >= maxAmount)
                {
                    return GiveItemErrors.MaxAmountReached;
                }

                //Make sure there is space again.
                if ((GetInventoryFilledSlots(storage) + item.Amount * item.AmountOfSlots) <= storage.MaxInvStorage)
                {
                    //Add.
                    oldItem.Amount += item.Amount;
                    if (oldItem.Amount == 0)
                    {
                        storage.Inventory.Remove(oldItem);
                    }
                    OnStorageGetItem?.Invoke(storage, new OnGetItemEventArgs(sentitem, amount));
                    OnStorageItemUpdateAmount?.Invoke(storage, new OnItemAmountUpdatedEventArgs(item.GetType(), oldItem.Amount));
                    return GiveItemErrors.Success;
                }
                else
                    return GiveItemErrors.NotEnoughSpace;
            }
        }

        /// <summary>
        /// Checks if user has certain item.
        /// </summary>
        /// <param name="storage">The storage that shall be checked.</param>
        /// <param name="item">The item Type</param>
        /// <returns>An array of IInventoryItem.</returns>
        public static IInventoryItem[] DoesInventoryHaveItem(IStorage storage, Type item)
        {
            if (storage.Inventory == null) storage.Inventory = new List<IInventoryItem>();
            return storage.Inventory.Where(x => x.GetType() == item).ToArray();
        }
        /// <summary>
        /// Checks if user has certain item.
        /// </summary>
        /// <param name="storage">The storage that shall be checked.</param>
        /// <param name="item">The item name. Commandname.</param>
        /// <returns>An array of IInventoryItem.</returns>
        public static IInventoryItem[] DoesInventoryHaveItem(IStorage storage, string item)
        {
            if (storage.Inventory == null) storage.Inventory = new List<IInventoryItem>();
            return storage.Inventory.Where(x => x.CommandFriendlyName.ToLower() == item.ToLower()).ToArray();
        }

        public static T[] DoesInventoryHaveItem<T>(IStorage storage)
        {
            if (storage.Inventory == null) storage.Inventory = new List<IInventoryItem>();
            return storage.Inventory.Where(x => x.GetType() == typeof(T)).Cast<T>().ToArray();
        }

        /// <summary>
        /// Converts a string to its equivelent Type of IInventoryItem.
        /// </summary>
        /// <param name="item">The item string</param>
        /// <returns>The IInventoryItem Type</returns>
        public static Type ParseInventoryItem(string item)
        {
            var allItems =
                Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(IInventoryItem).IsAssignableFrom(x) && x.IsClass).ToArray();

            foreach (var i in allItems)
            {
                IInventoryItem instance = (IInventoryItem)Activator.CreateInstance(i);
                if (instance.CommandFriendlyName == item)
                    return i;
            }
            return null;
        }

        /// <summary>
        /// Gets current filled slots.
        /// </summary>
        /// <param name="storage">The storage to check.</param>
        /// <returns>An integer of sum of taken slots.</returns>
        public static int GetInventoryFilledSlots(IStorage storage)
        {
            if (storage.Inventory == null) storage.Inventory = new List<IInventoryItem>();
            int value = 0;
            storage.Inventory.ForEach(x => value += x.AmountOfSlots * x.Amount);
            return value;
        }

        /// <summary>
        /// Removes an item from storage.
        /// </summary>
        /// <param name="storage">The storage to remove from.</param>
        /// <param name="item">The item to remove.</param>
        /// <param name="amount">Amount to be removed, -1 for all.</param>
        /// <param name="predicate">The predicate to be used, can be null for none</param>
        /// <returns>true if something was removed and false if nothing was removed.</returns>
        public static bool DeleteInventoryItem(IStorage storage, Type item, int amount = -1, Func<IInventoryItem, bool> predicate = null)
        {
            if (storage.Inventory == null) storage.Inventory = new List<IInventoryItem>();
            if (amount == -1)
            {
                IInventoryItem[] items = storage.Inventory.FindAll(x => x.GetType() == item).ToArray();
                if (storage.Inventory.RemoveAll(x => x.GetType() == item) > 0)
                {
                    foreach (var i in items)
                    {
                        OnStorageLoseItem?.Invoke(storage, new OnLoseItemEventArgs(i, amount));
                    }
                    OnStorageItemUpdateAmount?.Invoke(storage, new OnItemAmountUpdatedEventArgs(item, 0));
                    return true;
                }
                return false;
            }

            IInventoryItem itm = predicate != null ? storage.Inventory.Where(x => x.GetType() == item).SingleOrDefault(predicate) : storage.Inventory.SingleOrDefault(x => x.GetType() == item);
            if (itm != null)
            {
                itm.Amount -= amount;
                if (itm.Amount <= 0)
                    storage.Inventory.Remove(itm);

                OnStorageLoseItem?.Invoke(storage, new OnLoseItemEventArgs(itm, amount));
                OnStorageItemUpdateAmount?.Invoke(storage, new OnItemAmountUpdatedEventArgs(item, itm.Amount));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes an item from storage.
        /// </summary>
        /// <param name="storage">The storage to remove from.</param>
        /// <param name="amount">Amount to be removed, -1 for all.</param>
        /// <param name="predicate">The predicate to be used, can be null for none</param>
        /// <returns>true if something was removed and false if nothing was removed.</returns>
        public static bool DeleteInventoryItem<T>(IStorage storage, int amount = -1, Func<IInventoryItem, bool> predicate = null)
        {
            return DeleteInventoryItem(storage, typeof(T), amount, predicate);
        }

        public static void SetInventoryAmmount(IStorage storage, Type item, int amount)
        {
            DeleteInventoryItem(storage, item);
            GiveInventoryItem(storage, ItemTypeToNewObject(item), amount);
        }

        /// <summary>
        /// Creates a new object from Type.
        /// </summary>
        /// <param name="item">Item Type</param>
        /// <returns>Object of Type</returns>
        public static IInventoryItem ItemTypeToNewObject(Type item)
        {
            return (IInventoryItem)Activator.CreateInstance(item);
        }

        #region InventoryMovingManagement

        private static Dictionary<Client, KeyValuePair<IStorage, IStorage>> _activeInvsBeingManaged;
        public static void ShowInventoryManager(Client player, IStorage activeLeft, IStorage activeRight, string leftTitle, string rightTitle)
        {
            if (_activeInvsBeingManaged.ContainsKey(player))
            {
                API.shared.sendNotificationToPlayer(player, "You already have an inventory management window open.");
                return;
            }

            string[][] leftItems;
            if (activeRight.GetType() == typeof(BagItem))
            {
                leftItems = activeLeft.Inventory.Where(x => x.GetType() != typeof(BagItem))
                    .Select(x => new[] {x.Id.ToString(), x.LongName, x.CommandFriendlyName, x.Amount.ToString()})
                    .ToArray();
            }
            else
            {
                leftItems = activeLeft.Inventory.Select(x => new[] { x.Id.ToString(), x.LongName, x.CommandFriendlyName, x.Amount.ToString() })
                    .ToArray();
            }

            string[][] rightItems =
                activeRight.Inventory.Select(x => new[] {x.Id.ToString(), x.LongName, x.CommandFriendlyName, x.Amount.ToString()})
                    .ToArray();

            var leftJson = API.shared.toJson(leftItems);
            var rightJson = API.shared.toJson(rightItems);
            var usedLeft = GetInventoryFilledSlots(activeLeft) + "/" + activeLeft.MaxInvStorage;
            var usedRight = GetInventoryFilledSlots(activeRight) + "/" + activeRight.MaxInvStorage;
            API.shared.triggerClientEvent(player, "invmanagement_showmanager", leftJson, rightJson, leftTitle, rightTitle, usedLeft, usedRight);
            _activeInvsBeingManaged.Add(player, new KeyValuePair<IStorage, IStorage>(activeLeft, activeRight));
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "invmanagement_cancelled":
                    _activeInvsBeingManaged.Remove(sender);
                    API.sendNotificationToPlayer(sender, "Cancelled Inventory Management.");
                    break;
                   
                case "invmanagement_moveFromLeftToRight":
                    string id = (string)arguments[0];
                    string shortname = (string)arguments[1];
                    int amount;
                    if (!int.TryParse((string)arguments[2], out amount))
                    {
                        API.sendNotificationToPlayer(sender, "Invalid amount entered.");
                        return;
                    }
                    if (amount <= 0)
                    {
                        API.sendNotificationToPlayer(sender, "Amount must not be zero or negative.");
                        return;
                    }

                    //Make sure is managing.
                    if (!_activeInvsBeingManaged.ContainsKey(sender))
                    {
                        API.sendNotificationToPlayer(sender, "You aren't managing any inventory.");
                        return;
                    }

                    //Get the invs.
                    KeyValuePair<IStorage, IStorage> storages = _activeInvsBeingManaged.Get(sender);

                    //See if has item.
                    var item = InventoryManager.DoesInventoryHaveItem(storages.Key, shortname);
                    if (item.Length == 0)
                    {
                        API.sendNotificationToPlayer(sender, "That item type doesn't exist.");
                        return;
                    }
                    var playerItem = item.SingleOrDefault(x => x.Id.ToString() == id);
                    if (playerItem == null || playerItem.Amount < amount)
                    {
                        API.sendNotificationToPlayer(sender, "The source storage doesn't have that item or doesn't have that amount.");
                        return;
                    }

                    //Add to bag.
                    switch (InventoryManager.GiveInventoryItem(storages.Value, playerItem, amount, true))
                    {
                        case InventoryManager.GiveItemErrors.NotEnoughSpace:
                            API.sendNotificationToPlayer(sender, "There is no enough slots in target storage.");
                            break;
                        case InventoryManager.GiveItemErrors.MaxAmountReached:
                            API.sendNotificationToPlayer(sender, "Reached max amount of that item in target storage.");
                            break;
                        case InventoryManager.GiveItemErrors.HasSimilarItem:
                            API.sendNotificationToPlayer(sender, "You can't have 2 of this item with the same name, change it if possible.");
                            break;
                        case InventoryManager.GiveItemErrors.Success:
                            //Remove from player.
                            InventoryManager.DeleteInventoryItem(storages.Key, playerItem.GetType(), amount,
                                x => x.Id.ToString() == id && x.CommandFriendlyName == shortname);

                            //Send event done.
                            var usedLeft = GetInventoryFilledSlots(storages.Key) + "/" + storages.Key.MaxInvStorage;
                            var usedRight = GetInventoryFilledSlots(storages.Value) + "/" + storages.Value.MaxInvStorage;
                            API.triggerClientEvent(sender, "moveItemFromLeftToRightSuccess", id, shortname, amount, usedLeft, usedRight); //Id should be same cause it was already set since it was in player inv.
                            API.sendNotificationToPlayer(sender, $"The item ~g~{shortname}~w~ was moved sucessfully.");
                            break;
                    }
                    break;

                case "invmanagement_moveFromRightToLeft":

                    string rlid = (string)arguments[0];
                    string rlshortname = (string)arguments[1];
                    int rlamount;
                    if (!int.TryParse((string)arguments[2], out rlamount))
                    {
                        API.sendNotificationToPlayer(sender, "Invalid amount entered.");
                        return;
                    }
                    if (rlamount <= 0)
                    {
                        API.sendNotificationToPlayer(sender, "Amount must not be zero or negative.");
                        return;
                    }

                    //Make sure is managing.
                    if (!_activeInvsBeingManaged.ContainsKey(sender))
                    {
                        API.sendNotificationToPlayer(sender, "You aren't managing any inventory.");
                        return;
                    }

                    //Get the invs.
                    KeyValuePair<IStorage, IStorage> rlstorages = _activeInvsBeingManaged.Get(sender);

                    //See if has item.
                    var rlitem = InventoryManager.DoesInventoryHaveItem(rlstorages.Value, rlshortname);
                    if (rlitem.Length == 0)
                    {
                        API.sendNotificationToPlayer(sender, "That item type doesn't exist.");
                        return;
                    }
                    var rlplayerItem = rlitem.SingleOrDefault(x => x.Id.ToString() == rlid);
                    if (rlplayerItem == null || rlplayerItem.Amount < rlamount)
                    {
                        API.sendNotificationToPlayer(sender, "The source storage doesn't have that item or doesn't have that amount.");
                        return;
                    }

                    //Add to bag.
                    switch (InventoryManager.GiveInventoryItem(rlstorages.Key, rlplayerItem, rlamount, true))
                    {
                        case InventoryManager.GiveItemErrors.NotEnoughSpace:
                            API.sendNotificationToPlayer(sender, "There is no enough slots in target storage.");
                            break;
                        case InventoryManager.GiveItemErrors.MaxAmountReached:
                            API.sendNotificationToPlayer(sender, "Reached max amount of that item in target storage.");
                            break;
                        case InventoryManager.GiveItemErrors.HasSimilarItem:
                            API.sendNotificationToPlayer(sender, "You can't have 2 of this item with the same name, change it if possible.");
                            break;
                        case InventoryManager.GiveItemErrors.Success:
                            //Remove from player.
                            InventoryManager.DeleteInventoryItem(rlstorages.Value, rlplayerItem.GetType(), rlamount,
                                x => x.Id.ToString() == rlid && x.CommandFriendlyName == rlshortname);

                            //Send event done.
                            var usedLeft = GetInventoryFilledSlots(rlstorages.Key) + "/" + rlstorages.Key.MaxInvStorage;
                            var usedRight = GetInventoryFilledSlots(rlstorages.Value) + "/" + rlstorages.Value.MaxInvStorage;
                            API.triggerClientEvent(sender, "moveItemFromRightToLeftSuccess", rlid, rlshortname, rlamount, usedLeft, usedRight); //Id should be same cause it was already set since it was in player inv.
                            API.sendNotificationToPlayer(sender, $"The item ~g~{rlshortname}~w~ was moved sucessfully.");
                            break;
                    }

                    break;
            }
        }

        #endregion

        [Command("give")]
        public void give_cmd(Client player, string id, string item, int amount)
        {
            var targetClient = PlayerManager.ParseClient(id);
            if (targetClient == null)
            {
                API.sendNotificationToPlayer(player, "Target player not found.");
                return;
            }
            Character sender = player.GetCharacter();
            Character target = targetClient.GetCharacter();
            if (player.position.DistanceTo(targetClient.position) > 5f)
            {
                API.sendNotificationToPlayer(player, "You must be near the target player to give him an item.");
                return;
            }

            //Make sure he does have such amount.
            var sendersItem = DoesInventoryHaveItem(sender, item);
            if (sendersItem.Length != 1 || sendersItem[0].Amount < amount || amount <= 0)
            {
                API.sendNotificationToPlayer(player, "You don't have that item or you don't have that amount or there is more than 1 item with that name.");
                return;
            }

            if (sendersItem[0].CanBeGiven == false)
            {
                API.sendNotificationToPlayer(player, "That item cannot be given.");
                return;
            }


            //Give.
            switch (GiveInventoryItem(target, sendersItem[0], amount))
            {
                case GiveItemErrors.NotEnoughSpace:
                    API.sendNotificationToPlayer(player, "The target player doesn't have enough space in his inventory.");
                    API.sendNotificationToPlayer(targetClient,
                        "Someone has tried to give you an item but failed due to insufficient inventory.");
                    break;

                case GiveItemErrors.HasBlockingItem:
                    API.sendNotificationToPlayer(player, "The target player has a blocking item in hand.");
                    API.sendNotificationToPlayer(targetClient,
                        "You have a blocking item in-hand, place it somewhere first. /inv to find out what it is.");
                    break;
                case GiveItemErrors.MaxAmountReached:
                    API.sendNotificationToPlayer(player, "The target player reach max amount of that item.");
                    API.sendNotificationToPlayer(targetClient,
                        "You have reached the max amount of that item.");
                    break;

                case GiveItemErrors.Success:
                    API.sendNotificationToPlayer(player,
                        $"You have sucessfully given ~g~{amount}~w~ ~g~{sendersItem[0].LongName}~w~ to ~g~{target.CharacterName}~w~.");
                    API.sendNotificationToPlayer(targetClient,
                        $"You have receieved ~g~{amount}~w~ ~g~{sendersItem[0].LongName}~w~ from ~g~{sender.CharacterName}~w~.");

                    //Remove from their inv.
                    DeleteInventoryItem(sender, sendersItem[0].GetType(), amount, x => x == sendersItem[0]);

                    //RP
                    ChatManager.RoleplayMessage(player, $"gives an item to {target.rp_name()}", ChatManager.RoleplayMe);
                    break;
            }
        }

        [Command("drop")]
        public void drop_cmd(Client player, string item, int amount)
        {
            Character character = player.GetCharacter();

            //Get in inv.
            var sendersItem = DoesInventoryHaveItem(character, item);
            if (sendersItem.Length != 1 || sendersItem[0].Amount < amount || amount <= 0)
            {
                API.sendNotificationToPlayer(player, "You don't have that item or you don't have that amount or there is more than 1 item with that name.");
                return;
            }

            if (sendersItem[0].CanBeDropped == false)
            {
                API.sendNotificationToPlayer(player, "That item cannot be dropped.");
                return;
            }

            if (DeleteInventoryItem(character, sendersItem[0].GetType(), amount, x => x == sendersItem[0]))
            {
                API.sendNotificationToPlayer(player, "Item(s) was sucessfully dropped.");
                //RP
                ChatManager.RoleplayMessage(player, $"drops an item.", ChatManager.RoleplayMe);
            }
        }

        #region Stashing System: 
        private Dictionary<NetHandle, IInventoryItem> _stashedItems = new Dictionary<NetHandle, IInventoryItem>();

        [Command("stash")]
        public void stash_cmd(Client player, string item, int amount)
        {
            Character character = player.GetCharacter();

            //Get in inv.
            var sendersItem = DoesInventoryHaveItem(character, item);
            if (sendersItem.Length != 1 || sendersItem[0].Amount < amount || amount <= 0)
            {
                API.sendNotificationToPlayer(player, "You don't have that item or you don't have that amount.");
                return;
            }

            if (sendersItem[0].CanBeStashed == false)
            {
                API.sendNotificationToPlayer(player, "That item cannot be stashed.");
                return;
            }

            //Create object and add to list.
            var droppedObject = API.createObject(sendersItem[0].Object, player.position.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 0));
            _stashedItems.Add(droppedObject, CloneItem(sendersItem[0], amount));

            //Decrease.
            DeleteInventoryItem(character, sendersItem[0].GetType(), amount, x => x == sendersItem[0]);

            //Send message.
            API.sendNotificationToPlayer(player, $"You have sucessfully stashed ~g~{amount} {sendersItem[0].LongName}~w~. Use /pickupstash to take it.");
            //RP
            ChatManager.RoleplayMessage(player, $"stashs an item.", ChatManager.RoleplayMe);
        }

        [Command("pickupstash")]
        public void pickupstash_cmd(Client player)
        {
            //Check if near any stash.
            var items = _stashedItems.Where(x => API.getEntityPosition(x.Key).DistanceTo(player.position) <= 3).ToArray();
            if (!items.Any())
            {
                API.sendNotificationToPlayer(player, "You aren't near any stash.");
                return;
            }

            //Just get the first one and take it.
            Character character = player.GetCharacter();
            switch (GiveInventoryItem(character, items.First().Value, items.First().Value.Amount))
            {
                case GiveItemErrors.NotEnoughSpace:
                    API.sendNotificationToPlayer(player, "You don't have enough space in his inventory.");
                    break;

                case GiveItemErrors.HasBlockingItem:
                    API.sendNotificationToPlayer(player, "You have a blocking item in hand.");
                    break;
                case GiveItemErrors.MaxAmountReached:
                    API.sendNotificationToPlayer(player, "You have reached the max amount of that item.");
                    break;
                case GiveItemErrors.Success:
                    API.sendNotificationToPlayer(player,
                        $"You have sucessfully taken ~g~{items.First().Value.Amount}~w~ ~g~{items.First().Value.LongName}~w~ from the stash.");

                    //Remove object and item from list.
                    API.deleteEntity(items.First().Key);
                    _stashedItems.Remove(items.First().Key);

                    //RP
                    ChatManager.RoleplayMessage(player, $"picks an item from the ground.", ChatManager.RoleplayMe);
                    break;
            }
        }

        #endregion

        [Command("inventory", Alias = "inv")]
        public void showinventory_cmd(Client player)
        {
            //TODO: For now can be just text-based even though I'd recommend it to be a CEF.
            Character character = player.GetCharacter();

            //First the main thing.
            API.sendChatMessageToPlayer(player, "-------------------------------------------------------------");
            API.sendChatMessageToPlayer(player, $"[INVENTORY] {GetInventoryFilledSlots(character)}/{character.MaxInvStorage} Slots [INVENTORY]");
            
            //For Each item.
            foreach (var item in character.Inventory)
            {
                API.sendChatMessageToPlayer(player, $"* ~r~{item.LongName}~w~ [{item.CommandFriendlyName}] ({item.Amount}) Weights {item.AmountOfSlots} Slots" + (item.IsBlocking ? " [BLOCKING]" : ""));
            }

            //Ending
            API.sendChatMessageToPlayer(player, "-------------------------------------------------------------");
        }

        //TODO: TEST COMMAND.
        [Command("givemeitem")]
        public void GiveMeItem(Client player, string item, int amount)
        {
            Character character = player.GetCharacter();
            Type itemType = ParseInventoryItem(item);
            if (itemType != null)
            {
                var actualitem = ItemTypeToNewObject(itemType);
                API.sendChatMessageToPlayer(player, GiveInventoryItem(character, actualitem, amount).ToString());
            }
            else
                API.sendChatMessageToPlayer(player, "Invalid item name.");
        }
    }
}
