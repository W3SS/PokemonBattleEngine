﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Kermalis.PokemonBattleEngine.Data
{
    public static class PBELegalityChecker
    {
        // TODO: Include generation?
        // TODO: Sketch
        public static IEnumerable<PBEMove> GetLegalMoves(PBESpecies species, byte level)
        {
            IEnumerable<PBESpecies> evolutionChain = PBEPokemonData.Data[species].PreEvolutions.Concat(new[] { species });

            IEnumerable<PBEMove> moves = new PBEMove[0];
            foreach (PBESpecies pkmn in evolutionChain)
            {
                PBEPokemonData pData = PBEPokemonData.Data[pkmn];
                moves = moves.Union(pData.LevelUpMoves.Where(t => t.Item2 <= level).Select(t => t.Item1)) // Add level-up moves
                    .Union(pData.OtherMoves.Select(t => t.Item1)); // Add other moves
                if (PBEEventPokemon.Events.ContainsKey(pkmn))
                {
                    moves = moves.Union(PBEEventPokemon.Events[pkmn].SelectMany(e => e.Moves)); // Add event Pokémons' moves (TODO)
                }
            }

            return moves.Except(new[] { PBEMove.None });
        }

        public static void MoveLegalityCheck(PBESpecies species, byte level, IEnumerable<PBEMove> moves, PBESettings settings)
        {
            // Validate basic move rules first
            if (moves == null || moves.Count() != settings.NumMoves)
            {
                throw new ArgumentOutOfRangeException(nameof(moves), $"A Pokémon must have exactly {settings.NumMoves} moves.");
            }
            if (moves.Any(m => moves.Count(m2 => m != PBEMove.None && m == m2) > 1))
            {
                throw new ArgumentOutOfRangeException(nameof(moves), $"A Pokémon cannot have duplicate moves other than {PBEMove.None}.");
            }
            if (moves.All(m => m == PBEMove.None))
            {
                throw new ArgumentOutOfRangeException(nameof(moves), $"A Pokémon must have at least one move other than {PBEMove.None}.");
            }
            if (species == PBESpecies.Keldeo_Resolute && !moves.Contains(PBEMove.SecretSword))
            {
                throw new ArgumentOutOfRangeException(nameof(moves), $"{species} must have {PBEMove.SecretSword}.");
            }

            // Combine all moves from pre-evolutions
            IEnumerable<PBESpecies> evolutionChain = PBEPokemonData.Data[species].PreEvolutions.Concat(new[] { species });

            IEnumerable<Tuple<PBEMove, byte, PBEMoveObtainMethod>> levelUp = new Tuple<PBEMove, byte, PBEMoveObtainMethod>[0];
            IEnumerable<Tuple<PBEMove, PBEMoveObtainMethod>> other = new Tuple<PBEMove, PBEMoveObtainMethod>[0];
            foreach (PBESpecies pkmn in evolutionChain)
            {
                PBEPokemonData pData = PBEPokemonData.Data[pkmn];
                levelUp = levelUp.Union(pData.LevelUpMoves.Where(t => t.Item2 <= level));
                other = other.Union(pData.OtherMoves);
            }
            // TODO:
            IEnumerable<PBEMove> allAsMoves = GetLegalMoves(species, level);

            // Check if there's a move it cannot possibly learn
            foreach (PBEMove m in moves)
            {
                if (m != PBEMove.None && !allAsMoves.Contains(m))
                {
                    throw new ArgumentOutOfRangeException(nameof(moves), $"{species} cannot learn {m}.");
                }
            }

            // Check generational rules
            bool HasGen3Method(PBEMoveObtainMethod method)
            {
                return method.HasFlag(PBEMoveObtainMethod.LevelUp_RS)
                    || method.HasFlag(PBEMoveObtainMethod.LevelUp_FR)
                    || method.HasFlag(PBEMoveObtainMethod.LevelUp_LG)
                    || method.HasFlag(PBEMoveObtainMethod.LevelUp_E)
                    || method.HasFlag(PBEMoveObtainMethod.LevelUp_Colo)
                    || method.HasFlag(PBEMoveObtainMethod.LevelUp_XD)
                    || method.HasFlag(PBEMoveObtainMethod.TM_RSFRLGE)
                    || method.HasFlag(PBEMoveObtainMethod.HM_RSFRLGE)
                    || method.HasFlag(PBEMoveObtainMethod.MoveTutor_FRLG)
                    || method.HasFlag(PBEMoveObtainMethod.MoveTutor_E)
                    || method.HasFlag(PBEMoveObtainMethod.MoveTutor_XD)
                    || method.HasFlag(PBEMoveObtainMethod.EggMove_RSFRLG)
                    || method.HasFlag(PBEMoveObtainMethod.EggMove_E);
            }
            bool HasGen4Method(PBEMoveObtainMethod method)
            {
                return method.HasFlag(PBEMoveObtainMethod.LevelUp_DP)
                    || method.HasFlag(PBEMoveObtainMethod.LevelUp_Pt)
                    || method.HasFlag(PBEMoveObtainMethod.LevelUp_HGSS)
                    || method.HasFlag(PBEMoveObtainMethod.TM_DPPtHGSS)
                    || method.HasFlag(PBEMoveObtainMethod.HM_DPPt)
                    || method.HasFlag(PBEMoveObtainMethod.HM_HGSS)
                    || method.HasFlag(PBEMoveObtainMethod.MoveTutor_DP)
                    || method.HasFlag(PBEMoveObtainMethod.MoveTutor_Pt)
                    || method.HasFlag(PBEMoveObtainMethod.MoveTutor_HGSS)
                    || method.HasFlag(PBEMoveObtainMethod.EggMove_DPPt)
                    || method.HasFlag(PBEMoveObtainMethod.EggMove_HGSS);
            }
            bool HasGen5Method(PBEMoveObtainMethod method)
            {
                return method.HasFlag(PBEMoveObtainMethod.LevelUp_BW)
                    || method.HasFlag(PBEMoveObtainMethod.LevelUp_B2W2)
                    || method.HasFlag(PBEMoveObtainMethod.TM_BW)
                    || method.HasFlag(PBEMoveObtainMethod.TM_B2W2)
                    || method.HasFlag(PBEMoveObtainMethod.HM_BWB2W2)
                    || method.HasFlag(PBEMoveObtainMethod.MoveTutor_BW)
                    || method.HasFlag(PBEMoveObtainMethod.MoveTutor_B2W2)
                    || method.HasFlag(PBEMoveObtainMethod.EggMove_BWB2W2);
            }

            IEnumerable<Tuple<PBEMove, PBEMoveObtainMethod>> movesAsObtainMethods = levelUp.Where(t => moves.Contains(t.Item1)).Select(t => Tuple.Create(t.Item1, t.Item3)).Union(other.Where(t => moves.Contains(t.Item1)));

            // Check to see where the Pokémon DEFINITELY has been
            bool definitelyBeenInGeneration3 = false,
                definitelyBeenInGeneration4 = false,
                definitelyBeenInGeneration5 = false;
            foreach (Tuple<PBEMove, PBEMoveObtainMethod> m in movesAsObtainMethods)
            {
                bool gen3 = HasGen3Method(m.Item2),
                    gen4 = HasGen4Method(m.Item2),
                    gen5 = HasGen5Method(m.Item2);
                if (gen3 && !gen4 && !gen5)
                {
                    definitelyBeenInGeneration3 = true;
                }
                else if (!gen3 && gen4 && !gen5)
                {
                    definitelyBeenInGeneration4 = true;
                }
                else if (!gen3 && !gen4 && gen5)
                {
                    definitelyBeenInGeneration5 = true;
                }
            }

            // TODO: Check where the species was born
            // TODO: Check if moves make sense (example: learns a move in gen4 but was born in gen5/caught in dreamworld/is gen5 event)
            // TODO: Check if HMs were transferred
            // TODO: Check events for moves
            ;
        }

        public static void ValidateShell(this PBEPokemonShell shell, PBESettings settings)
        {
            // Validate Species
            PBEPokemonData pData;
            switch (shell.Species)
            {
                case PBESpecies.Darmanitan_Zen:
                case PBESpecies.Meloetta_Pirouette:
                    throw new ArgumentOutOfRangeException(nameof(shell.Species), $"{shell.Species} must be in its base forme.");
                default:
                    try
                    {
                        pData = PBEPokemonData.Data[shell.Species];
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Species), "Invalid species.");
                    }
                    break;
            }

            // Validate Nickname
            if (string.IsNullOrWhiteSpace(shell.Nickname))
            {
                throw new ArgumentOutOfRangeException(nameof(shell.Nickname), $"{nameof(shell.Nickname)} cannot be empty.");
            }
            if (shell.Nickname.Length > settings.MaxPokemonNameLength)
            {
                throw new ArgumentOutOfRangeException(nameof(shell.Nickname), $"{nameof(shell.Nickname)} cannot exceed {settings.MaxPokemonNameLength} characters.");
            }

            // Validate Level
            if (shell.Level < settings.MinLevel || shell.Level > settings.MaxLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(shell.Level), $"A {shell.Species}'s level must be at least {settings.MinLevel} and cannot exceed {settings.MaxLevel}.");
            }

            // Validate Ability
            if (!pData.HasAbility(shell.Ability))
            {
                throw new ArgumentOutOfRangeException(nameof(shell.Ability), $"{shell.Species} cannot have {shell.Ability}.");
            }

            // Validate Nature
            if (shell.Nature >= PBENature.MAX)
            {
                throw new ArgumentOutOfRangeException(nameof(shell.Nature), "Invalid nature.");
            }

            // Validate Gender
            if (shell.Gender >= PBEGender.MAX
                || (shell.Gender == PBEGender.Male && (pData.GenderRatio == PBEGenderRatio.M0_F1 || pData.GenderRatio == PBEGenderRatio.M0_F0))
                || (shell.Gender == PBEGender.Female && (pData.GenderRatio == PBEGenderRatio.M1_F0 || pData.GenderRatio == PBEGenderRatio.M0_F0))
                || (shell.Gender == PBEGender.Genderless && pData.GenderRatio != PBEGenderRatio.M0_F0)
                )
            {
                throw new ArgumentOutOfRangeException(nameof(shell.Gender), $"Invalid gender for {shell.Species}.");
            }

            // Validate Item
            if (shell.Item != PBEItem.None)
            {
                try
                {
                    PBEItemData iData = PBEItemData.Data[shell.Item];
                }
                catch
                {
                    throw new ArgumentOutOfRangeException(nameof(shell.Item), "Invalid item.");
                }
            }

            // Validate EVs
            if (shell.EVs == null || shell.EVs.Length != 6)
            {
                throw new ArgumentOutOfRangeException(nameof(shell.EVs), $"{nameof(shell.EVs)} array can only have a length of 6.");
            }
            if (shell.EVs.Select(e => (int)e).Sum() > settings.MaxTotalEVs)
            {
                throw new ArgumentOutOfRangeException(nameof(shell.EVs), $"Total EVs cannot exceed {settings.MaxTotalEVs}.");
            }
            // Validate IVs
            if (shell.IVs == null || shell.IVs.Length != 6)
            {
                throw new ArgumentOutOfRangeException(nameof(shell.IVs), $"{nameof(shell.IVs)} array can only have a length of 6.");
            }
            if (shell.IVs.Any(i => i > settings.MaxIVs))
            {
                throw new ArgumentOutOfRangeException(nameof(shell.IVs), $"Each IV cannot exceed {settings.MaxIVs}.");
            }

            // Validate Moves
            try
            {
                MoveLegalityCheck(shell.Species, shell.Level, shell.Moves, settings);
            }
            catch (Exception e)
            {
                throw new ArgumentOutOfRangeException(nameof(shell.Moves), e.Message);
            }

            // Validate PPUps
            if (shell.PPUps == null || shell.PPUps.Length != settings.NumMoves)
            {
                throw new ArgumentOutOfRangeException(nameof(shell.PPUps), $"{nameof(shell.PPUps)} array can only have a length of {settings.NumMoves}.");
            }
            if (shell.PPUps.Any(p => p > settings.MaxPPUps))
            {
                throw new ArgumentOutOfRangeException(nameof(shell.PPUps), $"Each PP-Up cannot exceed {settings.MaxPPUps}.");
            }

            // Validate Forme-Specific Requirements
            switch (shell.Species)
            {
                case PBESpecies.Shedinja:
                    if (shell.EVs[0] > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.EVs), $"{shell.Species} cannot have any HP EVs.");
                    }
                    break;
                case PBESpecies.Giratina:
                    if (shell.Item == PBEItem.GriseousOrb)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} cannot hold a {shell.Item}.");
                    }
                    break;
                case PBESpecies.Giratina_Origin:
                    if (shell.Item != PBEItem.GriseousOrb)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.GriseousOrb}.");
                    }
                    break;
                case PBESpecies.Arceus:
                    if (shell.Item == PBEItem.DracoPlate
                        || shell.Item == PBEItem.DreadPlate
                        || shell.Item == PBEItem.EarthPlate
                        || shell.Item == PBEItem.FistPlate
                        || shell.Item == PBEItem.FlamePlate
                        || shell.Item == PBEItem.IciclePlate
                        || shell.Item == PBEItem.InsectPlate
                        || shell.Item == PBEItem.IronPlate
                        || shell.Item == PBEItem.MeadowPlate
                        || shell.Item == PBEItem.MindPlate
                        || shell.Item == PBEItem.SkyPlate
                        || shell.Item == PBEItem.SplashPlate
                        || shell.Item == PBEItem.SpookyPlate
                        || shell.Item == PBEItem.StonePlate
                        || shell.Item == PBEItem.ToxicPlate
                        || shell.Item == PBEItem.ZapPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} cannot hold a {shell.Item}.");
                    }
                    break;
                case PBESpecies.Arceus_Bug:
                    if (shell.Item != PBEItem.InsectPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.InsectPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Dark:
                    if (shell.Item != PBEItem.DreadPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.DreadPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Dragon:
                    if (shell.Item != PBEItem.DracoPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.DracoPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Electric:
                    if (shell.Item != PBEItem.ZapPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.ZapPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Fighting:
                    if (shell.Item != PBEItem.FistPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.FistPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Fire:
                    if (shell.Item != PBEItem.FlamePlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.FlamePlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Flying:
                    if (shell.Item != PBEItem.SkyPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.SkyPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Ghost:
                    if (shell.Item != PBEItem.SpookyPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.SpookyPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Grass:
                    if (shell.Item != PBEItem.MeadowPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.MeadowPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Ground:
                    if (shell.Item != PBEItem.EarthPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.EarthPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Ice:
                    if (shell.Item != PBEItem.IciclePlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.IciclePlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Poison:
                    if (shell.Item != PBEItem.ToxicPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.ToxicPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Psychic:
                    if (shell.Item != PBEItem.MindPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.MindPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Rock:
                    if (shell.Item != PBEItem.StonePlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.StonePlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Steel:
                    if (shell.Item != PBEItem.IronPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.IronPlate}.");
                    }
                    break;
                case PBESpecies.Arceus_Water:
                    if (shell.Item != PBEItem.SplashPlate)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.SplashPlate}.");
                    }
                    break;
                case PBESpecies.Genesect:
                    if (shell.Item == PBEItem.BurnDrive
                        || shell.Item == PBEItem.ChillDrive
                        || shell.Item == PBEItem.DouseDrive
                        || shell.Item == PBEItem.ShockDrive)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} cannot hold a {shell.Item}.");
                    }
                    break;
                case PBESpecies.Genesect_Burn:
                    if (shell.Item != PBEItem.BurnDrive)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.BurnDrive}.");
                    }
                    break;
                case PBESpecies.Genesect_Chill:
                    if (shell.Item != PBEItem.ChillDrive)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.ChillDrive}.");
                    }
                    break;
                case PBESpecies.Genesect_Douse:
                    if (shell.Item != PBEItem.DouseDrive)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.DouseDrive}.");
                    }
                    break;
                case PBESpecies.Genesect_Shock:
                    if (shell.Item != PBEItem.ShockDrive)
                    {
                        throw new ArgumentOutOfRangeException(nameof(shell.Item), $"{shell.Species} must hold a {PBEItem.ShockDrive}.");
                    }
                    break;
            }
        }
    }
}
