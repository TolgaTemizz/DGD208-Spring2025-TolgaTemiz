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
        private const int CRITICAL_STAT_THRESHOLD = 20;
        private bool isGameActive;

        public Game()
        {
            pets = new List<Pet>();
            items = new List<Item>();
            InitializeItems();
            InitializeMainMenu();
            isGameActive = false;
        }

        private void InitializeItems()
        {
            items.Add(new Item("Dog Food", ItemType.Food, 20, new[] { PetType.Dog }, new[] { PetStat.Hunger }));
            items.Add(new Item("Cat Toy", ItemType.Toy, 15, new[] { PetType.Cat }, new[] { PetStat.Fun }));
            items.Add(new Item("Bird Seed", ItemType.Food, 15, new[] { PetType.Bird }, new[] { PetStat.Hunger }));
            items.Add(new Item("Fish Flakes", ItemType.Food, 10, new[] { PetType.Fish }, new[] { PetStat.Hunger }));
            items.Add(new Item("Rabbit Carrot", ItemType.Food, 15, new[] { PetType.Rabbit }, new[] { PetStat.Hunger }));
            items.Add(new Item("Pet Bed", ItemType.Bed, 25, Enum.GetValues<PetType>(), new[] { PetStat.Sleep }));
            items.Add(new Item("Ball", ItemType.Toy, 20, new[] { PetType.Dog, PetType.Cat }, new[] { PetStat.Fun }));
        }

        private void InitializeMainMenu()
        {
            mainMenu = new Menu("Pet Simulator", new List<string>
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
                isGameActive = false;
                int choice = mainMenu.Display();
                isGameActive = true;
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
            string name = Console.ReadLine();

            var pet = new Pet(name, selectedType);
            pet.PetDied += OnPetDied;
            pet.StatChanged += OnPetStatChanged;
            pets.Add(pet);

            _ = pet.DecreaseStatsAsync(isGameActive); // Start the stat decrease loop
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
                Console.WriteLine("\nYour Pets:");
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

        private void OnPetDied(object sender, EventArgs e)
        {
            var pet = (Pet)sender;
            pets.Remove(pet);
            Console.WriteLine($"\n{pet.Name} has died! :(");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void OnPetStatChanged(object sender, PetStat stat)
        {
            var pet = (Pet)sender;
            int currentValue = pet.Stats[stat];

            if (currentValue <= CRITICAL_STAT_THRESHOLD)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n⚠️ WARNING: {pet.Name}'s {stat} is critically low ({currentValue}%)!");
                Console.ResetColor();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
} 