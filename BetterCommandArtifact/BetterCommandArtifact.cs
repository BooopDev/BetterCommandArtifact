using BepInEx;
using RoR2;
using System.Linq;
using System.Collections.Generic;
using Option = RoR2.PickupPickerController.Option;
using UnityEngine.Networking;

namespace BetterCommandArtifact
{
    [BepInPlugin(Guid, ModName, Version), BepInDependency("com.rune580.riskofoptions")]
    public class BetterCommandArtifact : BaseUnityPlugin
    {
        const string
            ModName = "Better Command Artifact",
            Author = "Boooooop",
            Guid = "com." + Author + "." + "BetterCommandArtifact",
            Version = "1.2.0";
        
        public void OnEnable()
        {
            On.RoR2.PickupPickerController.SetOptionsFromPickupForCommandArtifact += OnSetOptionsCommand;
            On.RoR2.Artifacts.CommandArtifactManager.OnDropletHitGroundServer += OnDropletHitGround;
            RoR2Application.onLoad += OnLoad;
        }

        void OnLoad()
        {
            Settings.InitConfig(Config);
        }

        public void OnDisable()
        {
            On.RoR2.PickupPickerController.SetOptionsFromPickupForCommandArtifact -= OnSetOptionsCommand;
            On.RoR2.Artifacts.CommandArtifactManager.OnDropletHitGroundServer -= OnDropletHitGround;
        }

        [Server]
        void OnSetOptionsCommand(On.RoR2.PickupPickerController.orig_SetOptionsFromPickupForCommandArtifact orig, PickupPickerController self, PickupIndex pickupIndex)
        {
            if (!NetworkServer.active) return;

            self.SetOptionsServer(GetOptions(pickupIndex));
        }

        Option[] GetOptions(PickupIndex pickupIndex)
        {
            PickupIndex[] pickupGroup = PickupTransmutationManager.GetGroupFromPickupIndex(pickupIndex);

            if (pickupGroup == null) return new Option[1] { new Option { available = true, pickupIndex = pickupIndex } };

            List<Option> options = new();

            for (int i = 0; i < pickupGroup.Length; i++)
            {
                PickupIndex pickupFromGroup = pickupGroup[i];
                bool pickupAvailable = Settings.IncludeLockedItems.Value == 2 ? true : Run.instance.IsPickupAvailable(pickupFromGroup);
                options.Add(new Option { available = pickupAvailable, pickupIndex = pickupFromGroup });
            }

            var orderedOptions = from x in options orderby Run.instance.treasureRng.nextUlong select x;

            PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
            ItemTier pickupTier = pickupDef.itemTier;

            bool isEquipment = pickupDef.equipmentIndex != EquipmentIndex.None;
            int itemAmount = GetTierItemAmount(pickupTier, isEquipment, pickupDef.isLunar);

            if (Settings.IncludeLockedItems.Value == 1) return orderedOptions
                    .Where(x => x.available)
                    .Take(itemAmount)
                    .ToArray();

            return orderedOptions
                .Take(itemAmount)
                .ToArray();
        }

        private void OnDropletHitGround(On.RoR2.Artifacts.CommandArtifactManager.orig_OnDropletHitGroundServer orig, ref GenericPickupController.CreatePickupInfo createPickupInfo, ref bool shouldSpawn)
        {
            PickupIndex pickupIndex = createPickupInfo.pickupIndex;
            PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
            ItemTier pickupTier = pickupDef.itemTier;

            bool isEquipment = pickupDef.equipmentIndex != EquipmentIndex.None;
            bool isBossItem = pickupTier is ItemTier.Boss or ItemTier.VoidBoss;

            int itemAmount = GetTierItemAmount(pickupTier, isEquipment, pickupDef.isLunar);

            PickupIndex[] pickupGroup = PickupTransmutationManager.GetGroupFromPickupIndex(pickupIndex);

            if ((isBossItem && !Settings.MultipleBossItems.Value) || (isBossItem && pickupGroup == null) || itemAmount == 1)
                shouldSpawn = true;
            else
                orig(ref createPickupInfo, ref shouldSpawn);
        }

        int GetTierItemAmount(ItemTier itemTier, bool isEquipment, bool isLunar)
        {
            if (!Settings.UseItemTiers.Value) return Settings.DefaultItemsShown.Value;

            if (isEquipment) return isLunar ? Settings.LunarEquipmentItemsShown.Value : Settings.EquipmentItemsShown.Value;

            return itemTier switch
            {
                ItemTier.Tier1 => Settings.CommonItemsShown.Value,
                ItemTier.Tier2 => Settings.UncommonItemsShown.Value,
                ItemTier.Tier3 => Settings.LegendaryItemsShown.Value,
                ItemTier.Lunar => Settings.LunarItemsShown.Value,
                ItemTier.Boss => Settings.BossItemsShown.Value,
                ItemTier.VoidTier1 => Settings.CommonVoidItemsShown.Value,
                ItemTier.VoidTier2 => Settings.UncommonVoidItemsShown.Value,
                ItemTier.VoidTier3 => Settings.LegendaryVoidItemsShown.Value,
                ItemTier.VoidBoss => 1,
                _ => 1,
            };
        }
    }
}