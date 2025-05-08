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
        public event EventHandler<PetStat> StatChanged;
        public event EventHandler PetDied;

        public Pet(string name, PetType type)
        {
            Name = name;
            Type = type;
            Stats = new Dictionary<PetStat, int>
            {
                { PetStat.Hunger, 50 },
                { PetStat.Sleep, 50 },
                { PetStat.Fun, 50 }
            };
        }

        public async Task DecreaseStatsAsync(bool isGameActive)
        {
            while (true)
            {
                await Task.Delay(3000); // Decrease stats every 3 seconds
                
                if (!isGameActive)
                    continue;

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