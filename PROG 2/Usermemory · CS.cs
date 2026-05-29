
using System;
using System.Collections.Generic;

namespace CybersecurityBotGUI.Memory
{
    /// <summary>
    /// Stores information the user shares during the session so the chatbot
    /// can refer back to it later — satisfying the Memory and Recall requirement.
    /// Uses a generic Dictionary (generic collection requirement).
    /// </summary>
    public class UserMemory
    {
        // ─── Backing store ────────────────────────────────────────────────────────
        private readonly Dictionary<string, string> _memoryStore;

        // ─── Auto-properties ──────────────────────────────────────────────────────
        public string UserName { get; set; } = string.Empty;
        public string FavouriteTopic { get; set; } = string.Empty;
        public string LastTopic { get; set; } = string.Empty;
        public int MessageCount { get; private set; } = 0;

        // ─── Constructor ──────────────────────────────────────────────────────────
        public UserMemory()
        {
            _memoryStore = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        // ─── Store arbitrary key/value facts ─────────────────────────────────────
        public void Remember(string key, string value)
        {
            _memoryStore[key] = value;
        }

        // ─── Recall a stored fact ─────────────────────────────────────────────────
        public string Recall(string key)
        {
            return _memoryStore.TryGetValue(key, out string? val) ? val : string.Empty;
        }

        // ─── Check if a fact is remembered ───────────────────────────────────────
        public bool Has(string key) => _memoryStore.ContainsKey(key);

        // ─── Increment message counter ────────────────────────────────────────────
        public void IncrementMessage() => MessageCount++;

        // ─── Build a personalised memory hint for responses ───────────────────────
        /// <summary>
        /// Returns a sentence referencing the user's favourite topic when appropriate.
        /// Called by the ResponseEngine to enrich replies.
        /// </summary>
        public string GetTopicHint()
        {
            if (!string.IsNullOrWhiteSpace(FavouriteTopic))
                return $" As someone interested in {FavouriteTopic}, this is especially relevant for you.";
            return string.Empty;
        }

        /// <summary>
        /// Detects interest declarations like "I'm interested in privacy" and
        /// stores the topic automatically.
        /// </summary>
        public bool TryExtractInterest(string input)
        {
            string lower = input.ToLower();
            string[] triggers = { "interested in", "i like", "i love", "i care about", "i want to learn about", "curious about" };

            foreach (string trigger in triggers)
            {
                int idx = lower.IndexOf(trigger, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    string after = input.Substring(idx + trigger.Length).Trim().TrimEnd('.', '!', '?');
                    if (!string.IsNullOrWhiteSpace(after))
                    {
                        FavouriteTopic = after;
                        Remember("favourite_topic", after);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}