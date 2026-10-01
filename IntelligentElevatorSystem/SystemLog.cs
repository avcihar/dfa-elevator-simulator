using System;

namespace IntelligentElevatorSystem
{
    public class SystemLog
    {
        public string Timestamp { get; set; }
        public string Subsystem { get; set; } // Örn: "Dispatcher", "Cabin 1"
        public string Severity { get; set; }  // Örn: "INFO", "CRITICAL", "TRANSITION"
        public string Message { get; set; }

        public SystemLog(string subsystem, string severity, string message)
        {
            Timestamp = DateTime.Now.ToString("HH:mm:ss");
            Subsystem = subsystem;
            Severity = severity;
            Message = message;
        }

        public override string ToString()
        {
            return $"[{Timestamp}] [{Subsystem}] [{Severity}] : {Message}";
        }
    }
}