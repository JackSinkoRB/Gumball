using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CorePartLevel
    {

        [Header("Details")]
        [SerializeField] private int standardCurrencyCost;
        
        [Header("Slots")]
        [ConditionalField(nameof(hasSlot_ECU))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_ECU;
        [ConditionalField(nameof(hasSlot_Intake))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_Intake;
        [ConditionalField(nameof(hasSlot_Exhaust))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_Exhaust;
        [ConditionalField(nameof(hasSlot_NOS))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_NOS;
        [ConditionalField(nameof(hasSlot_Turbo))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_Turbo;
        
        [ConditionalField(nameof(hasSlot_Tyres))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_Tyres;
        [ConditionalField(nameof(hasSlot_Brakes))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_Brakes;
        [ConditionalField(nameof(hasSlot_Coilovers))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_Coilovers;
        
        [ConditionalField(nameof(hasSlot_Gearbox))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_Gearbox;
        [ConditionalField(nameof(hasSlot_Clutch))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_Clutch;
        [ConditionalField(nameof(hasSlot_Differential))]
        [SerializeField] private SubPart.SubPartRarity requiredRarity_Differential;
        
        [SerializeField, HideInInspector] private CorePart corePartBelongsTo;
        [SerializeField, HideInInspector] private bool hasSlot_ECU;
        [SerializeField, HideInInspector] private bool hasSlot_Intake;
        [SerializeField, HideInInspector] private bool hasSlot_Exhaust;
        [SerializeField, HideInInspector] private bool hasSlot_NOS;
        [SerializeField, HideInInspector] private bool hasSlot_Turbo;
        [SerializeField, HideInInspector] private bool hasSlot_Tyres;
        [SerializeField, HideInInspector] private bool hasSlot_Brakes;
        [SerializeField, HideInInspector] private bool hasSlot_Coilovers;
        [SerializeField, HideInInspector] private bool hasSlot_Gearbox;
        [SerializeField, HideInInspector] private bool hasSlot_Clutch;
        [SerializeField, HideInInspector] private bool hasSlot_Differential;

        public int StandardCurrencyCost => standardCurrencyCost;

#if UNITY_EDITOR
        public void SetupInspector(CorePart corePartBelongsTo)
        {
            this.corePartBelongsTo = corePartBelongsTo;

            hasSlot_ECU = this.corePartBelongsTo.Type == CorePart.PartType.ENGINE && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.ENGINE_ECU);
            hasSlot_Intake = this.corePartBelongsTo.Type == CorePart.PartType.ENGINE && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.ENGINE_Intake);
            hasSlot_Exhaust = this.corePartBelongsTo.Type == CorePart.PartType.ENGINE && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.ENGINE_Exhaust);
            hasSlot_NOS = this.corePartBelongsTo.Type == CorePart.PartType.ENGINE && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.ENGINE_NOS);
            hasSlot_Turbo = this.corePartBelongsTo.Type == CorePart.PartType.ENGINE && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.ENGINE_Turbo);
            hasSlot_Tyres = this.corePartBelongsTo.Type == CorePart.PartType.WHEELS && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.WHEELS_Tyres);
            hasSlot_Brakes = this.corePartBelongsTo.Type == CorePart.PartType.WHEELS && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.WHEELS_Brakes);
            hasSlot_Coilovers = this.corePartBelongsTo.Type == CorePart.PartType.WHEELS && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.WHEELS_Coilovers);
            hasSlot_Gearbox = this.corePartBelongsTo.Type == CorePart.PartType.DRIVETRAIN && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.DRIVETRAIN_Gearbox);
            hasSlot_Clutch = this.corePartBelongsTo.Type == CorePart.PartType.DRIVETRAIN && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.DRIVETRAIN_Clutch);
            hasSlot_Differential = this.corePartBelongsTo.Type == CorePart.PartType.DRIVETRAIN && this.corePartBelongsTo.SubPartSlots.Any(slot => slot.Type == SubPart.SubPartType.DRIVETRAIN_Differential);
        }
#endif
        
    }
}
