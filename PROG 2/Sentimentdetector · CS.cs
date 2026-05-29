using System;
using System.Collections.Generic;

namespace CybersecurityBotGUI.Sentiment
{
    /// <summary>
    /// Detects simple sentiment/mood from user input.
    /// Satisfies the Sentiment Detection requirement — the chatbot adjusts its
    /// responses based on the user's emotional tone.
    /// Uses a delegate (Action) to demonstrate delegate usage requirement.
    /// </summary>
    public class SentimentDetector
    {
        // ─── Sentiment enum ───────────────────────────────────────────────────────
        public enum Mood { Neutral, Worried, Curious, Frustrated, Happy, Overwhelmed }

        // ─── Delegate: called whenever a mood is detected ─────────────────────────
        public delegate void MoodDetectedHandler(Mood mood, string userName);
        public event MoodDetectedHandler? OnMoodDetected;

        // ─── Keyword maps ─────────────────────────────────────────────────────────
        private readonly Dictionary<Mood, string[]> _moodKeywords;

        public SentimentDetector()
        {
            _moodKeywords = new Dictionary<Mood, string[]>
            {
                { Mood.Worried,     new[] { "worried", "scared", "afraid", "anxious", "nervous", "concern", "unsafe", "danger", "threat", "hack" } },
                { Mood.Frustrated,  new[] { "frustrated", "annoyed", "angry", "useless", "stupid", "this is hard", "confusing", "don't understand", "difficult" } },
                { Mood.Curious,     new[] { "curious", "interesting", "tell me more", "how does", "what is", "explain", "i wonder", "learn", "want to know" } },
                { Mood.Happy,       new[] { "great", "awesome", "love it", "thanks", "thank you", "helpful", "perfect", "excellent", "amazing", "good" } },
                { Mood.Overwhelmed, new[] { "overwhelmed", "too much", "too many", "a lot", "complicated", "complex", "lost", "confused", "unsure" } },
            };
        }

        // ─── Detect mood from input ───────────────────────────────────────────────
        public Mood Detect(string input)
        {
            string lower = input.ToLower();

            foreach (var entry in _moodKeywords)
            {
                foreach (string keyword in entry.Value)
                {
                    if (lower.Contains(keyword))
                    {
                        OnMoodDetected?.Invoke(entry.Key, string.Empty);
                        return entry.Key;
                    }
                }
            }

            return Mood.Neutral;
        }

        // ─── Build empathetic prefix based on mood ────────────────────────────────
        /// <summary>
        /// Returns an empathetic opening sentence to prepend to the chatbot's
        /// response when a strong mood is detected.
        /// </summary>
        public static string GetEmpathyPrefix(Mood mood, string userName)
        {
            string name = string.IsNullOrWhiteSpace(userName) ? "there" : userName;

            return mood switch
            {
                Mood.Worried => $"It's completely understandable to feel worried, {name}. You're not alone — let me help. ",
                Mood.Frustrated => $"I hear you, {name}. Cybersecurity can feel overwhelming at first, but let's break it down simply. ",
                Mood.Curious => $"Great curiosity, {name}! That's exactly the right mindset for staying safe online. ",
                Mood.Happy => $"Glad to hear that, {name}! ",
                Mood.Overwhelmed => $"No worries, {name} — let's take it one step at a time. Here's a simple overview: ",
                _ => string.Empty,
            };
        }
    }
}