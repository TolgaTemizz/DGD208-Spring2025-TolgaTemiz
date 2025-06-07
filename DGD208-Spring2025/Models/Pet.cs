using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DGD208_Spring2025.Enums;

namespace DGD208_Spring2025.Models
{
    public class Pet
    {
        public string Name { get; private set; }
        public PetType Type { get; private set; }
        public Dictionary<PetStat, int> Stats { get; private set; }
        public int Level { get; private set; }
        public event EventHandler<PetStat>? StatChanged;
        public event EventHandler? PetDied;
        public event EventHandler<int>? LevelUp;

        public Pet(string name, PetType type)
        {
            Name = name;
            Type = type;
            Level = 1;
            Stats = new Dictionary<PetStat, int>
            {
                { PetStat.Hunger, 50 },
                { PetStat.Sleep, 50 },
                { PetStat.Fun, 50 }
            };
        }

        public void LoadLevel(int level)
        {
            Level = level;
        }

        public async Task DecreaseStatsAsync()
        {
            int secondsAlive = 0;
            while (true)
            {
                foreach (var stat in Stats.Keys.ToList())
                {
                    Stats[stat] = Math.Max(0, Stats[stat] - 1);
                    StatChanged?.Invoke(this, stat);

                    if (Stats[stat] == 0)
                    {
                        PetDied?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }

                secondsAlive++;
                if (secondsAlive >= 20)
                {
                    secondsAlive = 0;
                    LevelUp?.Invoke(this, ++Level);
                }

                await Task.Delay(1000);
            }
        }

        public void UseItem(Item item)
        {
            if (!item.CompatiblePets.Contains(Type))
                throw new InvalidOperationException($"This item is not compatible with {Type}");

            foreach (var stat in item.AffectedStats)
            {
                Stats[stat] = Math.Min(100, Stats[stat] + item.Value);
                StatChanged?.Invoke(this, stat);
            }
        }
    }
} 