﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;


namespace PickUpAndHaul
{
    public class PawnUnloadChecker
    {

        public static void CheckIfPawnShouldUnloadInventory(Pawn pawn, bool forced = false)
        {

            Job job = new Job(PickUpAndHaulJobDefOf.UnloadYourHauledInventory);
            CompHauledToInventory takenToInventory = pawn.TryGetComp<CompHauledToInventory>();
            if (takenToInventory == null)
            {
                Log.Warning(pawn + " cannot Pick Up And Haul. Does not inherit from BasePawn. Patch failed or mod incompatibility.");
                return;
            }
            HashSet<Thing> carriedThing = takenToInventory.GetHashSet();
            
            if (ModCompatibilityCheck.KnownConflict)
            {
                return;
            }

            if (pawn.Faction != Faction.OfPlayer || !pawn.RaceProps.Humanlike)
            {
                return;
            }
            
            if (carriedThing?.Count == 0 || pawn.inventory.innerContainer.Count == 0)
            {
                return;
            }

            if (carriedThing.Count != 0)
            {
                Thing thing = null;
                try
                {
                    carriedThing.Contains(thing);
                }
                catch (Exception arg)
                {
                    Log.Warning("There was an exception thrown by Pick Up And Haul. Pawn will clear inventory. \nException: " + arg);
                    carriedThing.Clear();
                    pawn.inventory.UnloadEverything = true;
                }
            }
            
            if (forced)
            {
                if (job.TryMakePreToilReservations(pawn))
                {
                    pawn.jobs.jobQueue.EnqueueFirst(job, new JobTag?(JobTag.Misc));
                    return;
                }
            }

            if (MassUtility.EncumbrancePercent(pawn) >= 0.90f || carriedThing.Count >= 2)
            {
                if (job.TryMakePreToilReservations(pawn))
                {
                    pawn.jobs.jobQueue.EnqueueFirst(job, new JobTag?(JobTag.Misc));
                    return;
                }
            }

            if (pawn.inventory.innerContainer?.Count >= 1)
            {
                foreach (Thing rottable in pawn.inventory.innerContainer)
                {
                    CompRottable compRottable = rottable.TryGetComp<CompRottable>();
                    if (compRottable != null)
                    {
                        //Log.Message(pawn + " compRottable" + rottable);
                        if (compRottable.TicksUntilRotAtCurrentTemp < 30000)
                        {
                            //Log.Message(pawn + " " + compRottable.TicksUntilRotAtCurrentTemp);
                            pawn.jobs.jobQueue.EnqueueFirst(job, new JobTag?(JobTag.Misc));
                            return;
                        }
                    }
                }
            }
            
            //if (carriedThing.Count >= 3) //try to unload a bit less aggressively
            //{
            //    if (job.TryMakePreToilReservations(pawn))
            //    {
            //        pawn.jobs.jobQueue.EnqueueFirst(job, new JobTag?(JobTag.Misc));
            //        return;
            //    }
            //}

            if (Find.TickManager.TicksGame % 50 == 0 && pawn.inventory.innerContainer.Count < carriedThing.Count)
            {
                Log.Warning("[PickUpAndHaul] " + pawn + " inventory was found out of sync with haul index. Pawn will drop their inventory.");
                carriedThing.Clear();
                pawn.inventory.UnloadEverything = true;                
            }
        }
    }

    [DefOf]
    public static class PickUpAndHaulJobDefOf
    {
        public static JobDef UnloadYourHauledInventory;
        public static JobDef HaulToInventory;
    }
}