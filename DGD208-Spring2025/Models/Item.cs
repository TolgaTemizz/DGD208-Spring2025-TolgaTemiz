using System;
using System.Collections.Generic;
using DGD208_Spring2025.Enums;

namespace DGD208_Spring2025.Models
{
    public class Item
    {
        public string Name { get; private set; }
        public int Value { get; private set; }
        public PetStat[] AffectedStats { get; private set; }
        public PetType[] CompatiblePets { get; private set; }

        public Item(string name, int value, PetStat[] affectedStats, PetType[] compatiblePets)
        {
            Name = name;
            Value = value;
            AffectedStats = affectedStats;
            CompatiblePets = compatiblePets;
        }
    }
} 