using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    DisplayCreatorInfo();
                    break;
                case 5:
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
                    Console.WriteLine($"\n{pet.Name} ({pet.Type})");
                    foreach (var stat in pet.Stats)
                    {
                        string statBar = new string('█', stat.Value / 10) + new string('░', 10 - (stat.Value / 10));
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
    }
} 