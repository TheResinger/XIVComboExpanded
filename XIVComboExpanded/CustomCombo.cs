﻿using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using System.Linq;

namespace XIVComboExpandedPlugin.Combos
{
    internal abstract class CustomCombo
    {
        #region static 

        private static IconReplacer? IconReplacer;
        protected static XIVComboExpandedConfiguration? Configuration;

        public static void Initialize(IconReplacer iconReplacer, XIVComboExpandedConfiguration configuration)
        {
            IconReplacer = iconReplacer!;
            Configuration = configuration!;
        }

        #endregion

        protected abstract CustomComboPreset Preset { get; }

        protected byte ClassID { get; }

        protected byte JobID { get; set; }

        protected virtual uint[] ActionIDs { get; set; }

        protected CustomCombo()
        {
            var presetInfo = Preset.GetInfo();
            JobID = presetInfo.JobID;
            ClassID = JobID switch
            {
                BLM.JobID => BLM.ClassID,
                BRD.JobID => BRD.ClassID,
                DRG.JobID => DRG.ClassID,
                MNK.JobID => MNK.ClassID,
                NIN.JobID => NIN.ClassID,
                PLD.JobID => PLD.ClassID,
                SCH.JobID => SCH.ClassID,
                SMN.JobID => SMN.ClassID,
                WAR.JobID => WAR.ClassID,
                WHM.JobID => WHM.ClassID,
                _ => 0xFF,
            };
            ActionIDs = presetInfo.ActionIDs;
        }

        public bool TryInvoke(uint actionID, uint lastComboMove, float comboTime, byte level, out uint newActionID)
        {
            newActionID = 0;

            if (!IsEnabled(Preset))
                return false;

            var classJobID = LocalPlayer?.ClassJob.Id;
            if (JobID != classJobID && ClassID != classJobID)
                return false;

            if (!ActionIDs.Contains(actionID))
                return false;

            var resultingActionID = Invoke(actionID, lastComboMove, comboTime, level);
            if (resultingActionID == 0 || actionID == resultingActionID)
                return false;

            newActionID = resultingActionID;
            return true;
        }

        protected abstract uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level);

        #region Passthru

        protected static uint OriginalHook(uint actionID) => IconReplacer!.OriginalHook(actionID);

        protected static PlayerCharacter? LocalPlayer => IconReplacer!.LocalPlayer;

        protected static GameObject? CurrentTarget => IconReplacer!.CurrentTarget;

        protected static bool IsEnabled(CustomComboPreset preset) => Configuration!.IsEnabled(preset);

        protected static bool HasCondition(ConditionFlag flag) => IconReplacer!.HasCondition(flag);

        protected static bool HasEffect(short effectID) => IconReplacer!.HasEffect(effectID);

        protected static bool TargetHasEffect(short effectID) => IconReplacer!.TargetHasEffect(effectID);

        protected static Status? FindEffect(short effectId) => IconReplacer!.FindEffect(effectId);

        protected static Status? FindTargetEffect(short effectId) => IconReplacer!.FindTargetEffect(effectId);

        protected static CooldownData GetCooldown(uint actionID) => IconReplacer!.GetCooldown(actionID);

        protected static T GetJobGauge<T>() where T : JobGaugeBase => IconReplacer!.GetJobGauge<T>();

        #endregion
    }
}