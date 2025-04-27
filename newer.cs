using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EmergencyResponseSim
{
    public class Incident
    {
        public string Type { get; }
        public string Location { get; }

        public Incident(string type, string location)
        {
            Type = type;
            Location = location;
        }

        public override string ToString()
        {
            return $"{Type} incident at {Location}";
        }
    }

    public abstract class EmergencyUnit
    {
        public string Name { get; protected set; }
        public int Speed { get; protected set; }

        protected EmergencyUnit(string name, int speed)
        {
            Name = name;
            Speed = speed;
        }

        public abstract bool CanHandle(string incidentType);
        public abstract void RespondToIncident(Incident incident);
    }

    public class Police : EmergencyUnit
    {
        public Police() : base("Police Unit", 80) { }

        public override bool CanHandle(string incidentType)
        {
            return incidentType.Equals("Crime", StringComparison.OrdinalIgnoreCase);
        }

        public override void RespondToIncident(Incident incident)
        {
            Console.WriteLine($"  -> {Name} responding to {incident}. Securing the area.");
        }
    }

    public class Firefighter : EmergencyUnit
    {
        public Firefighter() : base("Fire Engine", 60) { }

        public override bool CanHandle(string incidentType)
        {
            return incidentType.Equals("Fire", StringComparison.OrdinalIgnoreCase);
        }

        public override void RespondToIncident(Incident incident)
        {
            Console.WriteLine($"  -> {Name} responding to {incident}. Extinguishing the fire.");
        }
    }

    public class Ambulance : EmergencyUnit
    {
        public Ambulance() : base("Ambulance", 70) { }

        public override bool CanHandle(string incidentType)
        {
            return incidentType.Equals("Medical", StringComparison.OrdinalIgnoreCase);
        }

        public override void RespondToIncident(Incident incident)
        {
            Console.WriteLine($"  -> {Name} responding to {incident}. Providing medical assistance.");
        }
    }

    class Program
    {
        private static readonly Random random = new Random();
        private static readonly string[] incidentTypes = { "Fire", "Crime", "Medical" };
        private static readonly string[] locations = { "Downtown", "Uptown", "Suburb", "Industrial Park", "City Park", "Main St" };
        private static readonly List<EmergencyUnit> availableUnits = new List<EmergencyUnit>
            {
                new Police(),
                new Firefighter(),
                new Ambulance()
            };
        private const int MISS_CHANCE_DENOMINATOR = 10;


        static void Main(string[] args)
        {
            int score = 0;
            int totalRounds = 5;

            Console.WriteLine("--- Emergency Response Simulation Starting ---");
            Console.WriteLine($"Available Units: {string.Join(", ", availableUnits.Select(u => u.Name))}");
            Console.WriteLine($"Simulation will run for {totalRounds} rounds.");
            Console.WriteLine($"There is a 1 in {MISS_CHANCE_DENOMINATOR} chance a unit might fail to respond even if available.\n");

            for (int roundNum = 1; roundNum <= totalRounds; roundNum++)
            {
                Console.WriteLine($"--- Round {roundNum} ---");

                Console.WriteLine("Choose incident generation method for this round:");
                Console.WriteLine("  1. Generate 5 Random Incidents");
                Console.WriteLine("  2. Enter 1 Custom Incident");
                Console.Write("Enter choice (1 or 2): ");
                string choice = Console.ReadLine()?.Trim();
                Console.WriteLine();

                List<Incident> incidentsThisRound = new List<Incident>();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("Generating 5 random incidents...");
                        for (int i = 0; i < 5; i++)
                        {
                            incidentsThisRound.Add(GenerateRandomIncident());
                        }
                        break;
                    case "2":
                        Console.WriteLine("Entering 1 custom incident...");
                        incidentsThisRound.Add(GetCustomIncident());
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Generating 5 random incidents by default...");
                        for (int i = 0; i < 5; i++)
                        {
                            incidentsThisRound.Add(GenerateRandomIncident());
                        }
                        break;
                }

                Console.WriteLine($"Processing {incidentsThisRound.Count} incident(s) for Round {roundNum}:");
                int incidentCounter = 0;
                foreach (Incident currentIncident in incidentsThisRound)
                {
                    incidentCounter++;
                    Console.WriteLine($"\nProcessing Incident {incidentCounter}/{incidentsThisRound.Count}: {currentIncident}");

                    EmergencyUnit potentialHandler = FindHandlerUnit(currentIncident);

                    if (potentialHandler != null)
                    {
                        bool missedDispatch = random.Next(MISS_CHANCE_DENOMINATOR) == 0;

                        if (missedDispatch)
                        {
                            Console.WriteLine($"  -> {potentialHandler.Name} was available but FAILED to respond to the dispatch!");
                            score -= 5;
                            Console.WriteLine("  Missed response! -5 points.");
                        }
                        else
                        {
                            potentialHandler.RespondToIncident(currentIncident);
                            score += 10;
                            Console.WriteLine("  Incident handled correctly! +10 points.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  -> No available unit can handle a {currentIncident.Type} incident!");
                        score -= 5;
                        Console.WriteLine("  Incident could not be handled. -5 points.");
                    }

                    if (incidentsThisRound.Count > 1)
                    {
                        Thread.Sleep(500);
                    }
                }

                Console.WriteLine($"\n--- End of Round {roundNum} ---");
                Console.WriteLine($"Current Score: {score}\n");

                Thread.Sleep(1000);
            }

            Console.WriteLine("--- Simulation Ended ---");
            Console.WriteLine($"Final Score: {score}");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static EmergencyUnit FindHandlerUnit(Incident incident)
        {
            foreach (var unit in availableUnits)
            {
                if (unit.CanHandle(incident.Type))
                {
                    return unit;
                }
            }
            return null;
        }

        private static Incident GenerateRandomIncident()
        {
            string randomType = incidentTypes[random.Next(incidentTypes.Length)];
            string randomLocation = locations[random.Next(locations.Length)];
            return new Incident(randomType, randomLocation);
        }

        private static Incident GetCustomIncident()
        {
            string incidentType = "";
            bool isValidType = false;
            string allowedTypesPrompt = string.Join(", ", incidentTypes);

            while (!isValidType)
            {
                Console.Write($"Enter Incident Type ({allowedTypesPrompt}): ");
                incidentType = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(incidentType) &&
                    incidentTypes.Contains(incidentType, StringComparer.OrdinalIgnoreCase))
                {
                    incidentType = char.ToUpper(incidentType[0]) + incidentType.Substring(1).ToLower();
                    isValidType = true;
                }
                else
                {
                    Console.WriteLine($"Invalid incident type. Please enter one of: {allowedTypesPrompt}.");
                }
            }

            Console.Write("Enter Incident Location: ");
            string location = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(location))
            {
                location = "Unknown Location";
                Console.WriteLine("Location not specified, using 'Unknown Location'.");
            }

            return new Incident(incidentType, location);
        }
    }
}
