using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using DGD208_Spring2025.Enums;
using DGD208_Spring2025.Models;
using DGD208_Spring2025.UI;

namespace DGD208_Spring2025
{
    public class Game
    {
        private List<Pet> pets;
        private List<Item> items;
        private Menu mainMenu;
        private bool isRunning;
        private readonly int[] WARNING_THRESHOLDS = { 20, 15, 10, 5 };
        private Dictionary<Pet, Dictionary<PetStat, int>> lastWarningLevels;
        private const string SAVE_FILE = "pets_save.json";

        public Game()
        {
            pets = new List<Pet>();
            items = new List<Item>();
            lastWarningLevels = new Dictionary<Pet, Dictionary<PetStat, int>>();
            InitializeItems();
            InitializeMenus();
        }

        private void InitializeItems()
        {
            // Köpek için itemler
            items.Add(new Item("Köpek Maması", 20, new[] { PetStat.Hunger }, new[] { PetType.Dog }));
            items.Add(new Item("Köpek Yatağı", 20, new[] { PetStat.Sleep }, new[] { PetType.Dog }));
            items.Add(new Item("Köpek Topu", 20, new[] { PetStat.Fun }, new[] { PetType.Dog }));

            // Kedi için itemler
            items.Add(new Item("Kedi Maması", 20, new[] { PetStat.Hunger }, new[] { PetType.Cat }));
            items.Add(new Item("Kedi Yatağı", 20, new[] { PetStat.Sleep }, new[] { PetType.Cat }));
            items.Add(new Item("Kedi Oyuncağı", 20, new[] { PetStat.Fun }, new[] { PetType.Cat }));

            // Kuş için itemler
            items.Add(new Item("Kuş Yemi", 20, new[] { PetStat.Hunger }, new[] { PetType.Bird }));
            items.Add(new Item("Kuş Yuvası", 20, new[] { PetStat.Sleep }, new[] { PetType.Bird }));
            items.Add(new Item("Kuş Oyuncağı", 20, new[] { PetStat.Fun }, new[] { PetType.Bird }));

            // Balık için itemler
            items.Add(new Item("Balık Yemi", 20, new[] { PetStat.Hunger }, new[] { PetType.Fish }));
            items.Add(new Item("Balık Yuvası", 20, new[] { PetStat.Sleep }, new[] { PetType.Fish }));
            items.Add(new Item("Balık Oyuncağı", 20, new[] { PetStat.Fun }, new[] { PetType.Fish }));

            // Tavşan için itemler
            items.Add(new Item("Havuç", 20, new[] { PetStat.Hunger }, new[] { PetType.Rabbit }));
            items.Add(new Item("Tavşan Yuvası", 20, new[] { PetStat.Sleep }, new[] { PetType.Rabbit }));
            items.Add(new Item("Tavşan Oyuncağı", 20, new[] { PetStat.Fun }, new[] { PetType.Rabbit }));
        }

        private void InitializeMenus()
        {
            mainMenu = new Menu("Ana Menü", new List<string>
            {
                "Adopt a Pet",
                "View Pets",
                "Use Item",
                "Save Game",
                "Load Game",
                "Display Creator Info",
                "Exit"
            });
        }

        public async Task RunAsync()
        {
            isRunning = true;
            while (isRunning)
            {
                int choice = mainMenu.Display();
                await ProcessChoiceAsync(choice);
            }
        }

        private async Task ProcessChoiceAsync(int choice)
        {
            switch (choice)
            {
                case 1:
                    await AdoptPetAsync();
                    break;
                case 2:
                    DisplayPets();
                    break;
                case 3:
                    UseItem();
                    break;
                case 4:
                    SaveGame();
                    break;
                case 5:
                    LoadGame();
                    break;
                case 6:
                    DisplayCreatorInfo();
                    break;
                case 7:
                    isRunning = false;
                    break;
            }
        }

        private async Task AdoptPetAsync()
        {
            var petTypes = Enum.GetValues<PetType>();
            var petOptions = petTypes.Select(p => p.ToString()).ToList();
            var petMenu = new Menu("Choose a Pet Type", petOptions);
            
            int choice = petMenu.Display();
            PetType selectedType = petTypes[choice - 1];

            Console.Write("Enter pet name: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Invalid name!");
                return;
            }

            var pet = new Pet(name, selectedType);
            pet.PetDied += OnPetDied;
            pet.StatChanged += OnPetStatChanged;
            pet.LevelUp += OnPetLevelUp;
            pets.Add(pet);

            // Initialize warning levels for the new pet
            lastWarningLevels[pet] = new Dictionary<PetStat, int>();
            foreach (PetStat stat in Enum.GetValues<PetStat>())
            {
                lastWarningLevels[pet][stat] = 100; // Start above all thresholds
            }

            _ = pet.DecreaseStatsAsync(); // Start the stat decrease loop
            Console.WriteLine($"\n{name} has been adopted!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void DisplayPets()
        {
            if (!pets.Any())
            {
                Console.WriteLine("\nYou don't have any pets yet!");
            }
            else
            {
                Console.WriteLine($"\nYour Pets (Last Update: {DateTime.Now:HH:mm:ss}):");
                foreach (var pet in pets)
                {
                    bool hasLowStats = pet.Stats.Values.Any(stat => stat < 30);
                    Console.WriteLine($"\n{pet.Name} ({pet.Type}) Lvl{pet.Level} {(hasLowStats ? "):" : "(:")}");
                    
                    // Display ASCII art based on pet type
                    switch (pet.Type)
                    {
                        case PetType.Dog:
                            Console.WriteLine(@"  / \__");
                            Console.WriteLine(@" (    @\___");
                            Console.WriteLine(@" /         O");
                            Console.WriteLine(@"/   (_____/");
                            Console.WriteLine(@"/_____/   U");
                            break;
                        case PetType.Cat:
                            Console.WriteLine(@" /\_/\");
                            Console.WriteLine(@"( o.o )");
                            Console.WriteLine(@" > ^ <");
                            break;
                        case PetType.Bird:
                            Console.WriteLine(@"              __");
                            Console.WriteLine(@"             /'{>");
                            Console.WriteLine(@"         ____) (____");
                            Console.WriteLine(@"       //'--;   ;--'\\");
                            Console.WriteLine(@"      ///////\_/\\\\\\\");
                            Console.WriteLine(@"jgs          m m");
                            break;
                        case PetType.Fish:
                            Console.WriteLine(@"               O  o");
                            Console.WriteLine(@"          _\_   o");
                            Console.WriteLine(@">('>   \\/  o\ .");
                            Console.WriteLine(@"       //\___=");
                            Console.WriteLine(@"          ''");
                            break;
                        case PetType.Rabbit:
                            Console.WriteLine(@"  (\_/)");
                            Console.WriteLine(@" (='.'=)");
                            Console.WriteLine(@"("")_("")");
                            break;
                    }
                    
                    Console.WriteLine("\nStats:");
                    foreach (var stat in pet.Stats)
                    {
                        string statBar = new string('*', stat.Value / 10) + new string('.', 10 - (stat.Value / 10));
                        Console.WriteLine($"{stat.Key}: [{statBar}] {stat.Value}%");
                    }
                }
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private void UseItem()
        {
            if (!pets.Any())
            {
                Console.WriteLine("\nYou don't have any pets to use items on!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var petMenu = new Menu("Select a Pet", pets.Select(p => p.Name).ToList());
            int petChoice = petMenu.Display();
            var selectedPet = pets[petChoice - 1];

            var compatibleItems = items.Where(i => i.CompatiblePets.Contains(selectedPet.Type)).ToList();
            var itemMenu = new Menu("Select an Item", compatibleItems.Select(i => i.Name).ToList());
            int itemChoice = itemMenu.Display();
            var selectedItem = compatibleItems[itemChoice - 1];

            try
            {
                // Display item ASCII art based on item name
                Console.WriteLine("\nUsing item:");
                if (selectedItem.Name.Contains("Maması") || selectedItem.Name.Contains("Yemi") || selectedItem.Name == "Havuç")
                {
                    Console.WriteLine(@"   ___");
                    Console.WriteLine(@"  /   \");
                    Console.WriteLine(@" /     \");
                    Console.WriteLine(@"/       \");
                    Console.WriteLine(@"\       /");
                    Console.WriteLine(@" \_____/");
                }
                else if (selectedItem.Name.Contains("Yatağı") || selectedItem.Name.Contains("Yuvası"))
                {
                    Console.WriteLine(@"  ______");
                    Console.WriteLine(@" /      \");
                    Console.WriteLine(@"/        \");
                    Console.WriteLine(@"|        |");
                    Console.WriteLine(@"|        |");
                    Console.WriteLine(@" \______/");
                }
                else if (selectedItem.Name.Contains("Topu") || selectedItem.Name.Contains("Oyuncağı"))
                {
                    Console.WriteLine(@"    ____");
                    Console.WriteLine(@"   /    \");
                    Console.WriteLine(@"  /      \");
                    Console.WriteLine(@" /        \");
                    Console.WriteLine(@" \        /");
                    Console.WriteLine(@"  \______/");
                }

                selectedPet.UseItem(selectedItem);
                Console.WriteLine($"\nUsed {selectedItem.Name} on {selectedPet.Name}!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void DisplayCreatorInfo()
        {
            Console.WriteLine("\nProject Creator: Tolga Temiz");
            Console.WriteLine("Student Number: 225040086");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private void OnPetDied(object? sender, EventArgs e)
        {
            if (sender is Pet pet)
            {
                pets.Remove(pet);
                lastWarningLevels.Remove(pet);
                Console.WriteLine($"\n{pet.Name} has died! :(");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private void OnPetStatChanged(object? sender, PetStat stat)
        {
            if (sender is Pet pet)
            {
                int currentValue = pet.Stats[stat];
                int lastWarningLevel = lastWarningLevels[pet][stat];

                // Find the highest threshold that we've crossed
                int newWarningLevel = WARNING_THRESHOLDS.FirstOrDefault(t => currentValue <= t && lastWarningLevel > t);

                if (newWarningLevel > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n⚠️ WARNING: {pet.Name}'s {stat} is at {currentValue}%!");
                    Console.ResetColor();
                    lastWarningLevels[pet][stat] = newWarningLevel;
                }
            }
        }

        private void OnPetLevelUp(object? sender, int newLevel)
        {
            if (sender is Pet pet)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n🎉 {pet.Name} has reached Lvl{newLevel}! 🎉");
                Console.ResetColor();
            }
        }

        private void SaveGame()
        {
            try
            {
                var saveData = new SaveData
                {
                    Pets = pets.Select(p => new PetData
                    {
                        Name = p.Name,
                        Type = p.Type.ToString(),
                        Level = p.Level,
                        Stats = p.Stats.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value)
                    }).ToList()
                };

                string jsonString = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SAVE_FILE, jsonString);
                Console.WriteLine("\nGame saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError saving game: {ex.Message}");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void LoadGame()
        {
            try
            {
                if (!File.Exists(SAVE_FILE))
                {
                    Console.WriteLine("\nNo save file found!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                string jsonString = File.ReadAllText(SAVE_FILE);
                var saveData = JsonSerializer.Deserialize<SaveData>(jsonString);

                if (saveData?.Pets == null)
                {
                    Console.WriteLine("\nInvalid save file!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Clear current pets
                pets.Clear();
                lastWarningLevels.Clear();

                // Load pets from save data
                foreach (var petData in saveData.Pets)
                {
                    if (!Enum.TryParse<PetType>(petData.Type, out var petType))
                    {
                        Console.WriteLine($"\nInvalid pet type in save file: {petData.Type}");
                        continue;
                    }

                    var pet = new Pet(petData.Name, petType);
                    pet.LoadLevel(petData.Level);
                    
                    // Restore stats
                    foreach (var stat in petData.Stats)
                    {
                        if (Enum.TryParse<PetStat>(stat.Key, out var statType))
                        {
                            pet.Stats[statType] = stat.Value;
                        }
                    }

                    pet.PetDied += OnPetDied;
                    pet.StatChanged += OnPetStatChanged;
                    pet.LevelUp += OnPetLevelUp;
                    pets.Add(pet);

                    // Initialize warning levels
                    lastWarningLevels[pet] = new Dictionary<PetStat, int>();
                    foreach (PetStat stat in Enum.GetValues<PetStat>())
                    {
                        lastWarningLevels[pet][stat] = pet.Stats[stat];
                    }

                    _ = pet.DecreaseStatsAsync();
                }

                Console.WriteLine("\nGame loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError loading game: {ex.Message}");
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private class SaveData
        {
            public List<PetData> Pets { get; set; } = new List<PetData>();
        }

        private class PetData
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public int Level { get; set; }
            public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>();
        }
    }
} 