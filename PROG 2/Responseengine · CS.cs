using System;
using System.Collections.Generic;
using CybersecurityBotGUI.Memory;
using CybersecurityBotGUI.Sentiment;

namespace CybersecurityBotGUI.Responses
{
    /// <summary>
    /// Core response engine — extended from Part 1.
    /// Features:
    ///   • Random responses for common queries (uses List&lt;string&gt;)
    ///   • Conversation flow / follow-up detection
    ///   • Memory integration (personalised responses)
    ///   • Sentiment-aware empathy prefix
    ///   • Keyword recognition
    ///   • Error handling / default fallback
    /// Uses delegates for extensibility.
    /// </summary>
    public class ResponseEngine
    {
        public delegate string FallbackHandler(string input, string userName);
        private FallbackHandler _fallback;

        private readonly Dictionary<string, List<string>> _randomPools;
        private readonly Dictionary<string, string> _keywordResponses;
        private readonly HashSet<string> _followUpPhrases;
        private readonly Random _rng = new Random();

        public ResponseEngine()
        {
            _fallback = DefaultFallback;
            _randomPools = BuildRandomPools();
            _keywordResponses = BuildKeywordResponses();
            _followUpPhrases = BuildFollowUpSet();
        }

        public void SetFallbackHandler(FallbackHandler handler) => _fallback = handler;

        public string GetResponse(string input, UserMemory memory, SentimentDetector.Mood mood)
        {
            memory.IncrementMessage();
            string lower = input.ToLower().Trim();
            string name = memory.UserName;
            string empathy = SentimentDetector.GetEmpathyPrefix(mood, name);

            // Check if user is declaring an interest
            if (memory.TryExtractInterest(input))
            {
                string topic = memory.FavouriteTopic;
                memory.LastTopic = topic;
                return $"{empathy}Great! I'll remember that you're interested in {topic}. " +
                       $"It's a crucial part of staying safe online.\n\n" +
                       GetTopicResponse(topic, memory);
            }

            // Follow-up / conversation flow
            if (IsFollowUp(lower) && !string.IsNullOrWhiteSpace(memory.LastTopic))
            {
                return $"{empathy}Sure! Here's more on {memory.LastTopic}:\n\n" +
                       GetTopicResponse(memory.LastTopic, memory);
            }

            // Random pools (phishing tips, password tips, scam tips)
            foreach (var pool in _randomPools)
            {
                if (lower.Contains(pool.Key))
                {
                    var responses = pool.Value;
                    string picked = responses[_rng.Next(responses.Count)];
                    UpdateLastTopic(pool.Key, memory);
                    return empathy + picked + memory.GetTopicHint();
                }
            }

            // Fixed keyword responses
            foreach (var entry in _keywordResponses)
            {
                if (lower.Contains(entry.Key))
                {
                    UpdateLastTopic(entry.Key, memory);
                    return empathy + entry.Value + memory.GetTopicHint();
                }
            }

            // Scam / worried conversation flow
            if (lower.Contains("scam") || lower.Contains("online scam"))
            {
                memory.LastTopic = "scams";
                return $"{empathy}It's completely understandable to feel that way. Scammers can be very " +
                       "convincing. Let me share some tips to help you stay safe.\n\n" +
                       "• Never give personal or banking info to unsolicited callers.\n" +
                       "• Verify any 'urgent' messages through official channels.\n" +
                       "• If an offer seems too good to be true — it probably is.\n" +
                       "• Report scams to the South African Police Service (SAPS) or www.cybercrime.gov.za.";
            }

            // Fallback via delegate
            return _fallback(input, name);
        }

        private string GetTopicResponse(string topic, UserMemory memory)
        {
            string lower = topic.ToLower();
            foreach (var entry in _keywordResponses)
            {
                if (lower.Contains(entry.Key))
                    return entry.Value;
            }
            foreach (var pool in _randomPools)
            {
                if (lower.Contains(pool.Key))
                    return _randomPools[pool.Key][_rng.Next(_randomPools[pool.Key].Count)];
            }
            return $"I have general advice on {topic}: always stay informed, keep software updated, " +
                   "and use strong unique passwords for every account.";
        }

        private static void UpdateLastTopic(string key, UserMemory memory)
        {
            memory.LastTopic = key;
        }

        private bool IsFollowUp(string lower)
        {
            foreach (string phrase in _followUpPhrases)
            {
                if (lower.Contains(phrase))
                    return true;
            }
            return false;
        }

        private static string DefaultFallback(string input, string userName)
        {
            string name = string.IsNullOrWhiteSpace(userName) ? "there" : userName;
            return $"I'm not sure I understand that, {name}. Can you try rephrasing?\n\n" +
                   "Try asking about: passwords, phishing, scams, privacy, malware, 2FA, encryption, or safe browsing.";
        }

        private Dictionary<string, List<string>> BuildRandomPools()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["phishing tip"] = new List<string>
                {
                    "🎣 Tip: Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                    "🎣 Tip: Always hover over links before clicking — the real URL appears at the bottom of your browser.",
                    "🎣 Tip: Legitimate banks will NEVER ask for your PIN or password via email or SMS.",
                    "🎣 Tip: Check the sender's email address carefully — phishers often use addresses like 'support@paypa1.com'.",
                    "🎣 Tip: If an email creates urgency ('Act NOW or your account is closed!'), treat it as a red flag.",
                },

                ["password tip"] = new List<string>
                {
                    "🔑 Tip: Use a passphrase of 4+ random words — e.g., 'PurpleTiger$Rain42' — easier to remember, hard to crack.",
                    "🔑 Tip: Never reuse the same password on multiple sites. If one gets breached, all your accounts are at risk.",
                    "🔑 Tip: Use a reputable password manager (Bitwarden, 1Password) — you only need to remember one master password.",
                    "🔑 Tip: Change passwords immediately if you suspect an account has been compromised.",
                    "🔑 Tip: Avoid dictionary words, names, or birthdates — attackers use these first in brute-force attacks.",
                },

                ["scam tip"] = new List<string>
                {
                    "⚠️ Tip: If someone calls claiming to be from Microsoft or your bank asking for remote access — hang up immediately.",
                    "⚠️ Tip: Verify prize or lottery wins by contacting the organisation directly through their official website.",
                    "⚠️ Tip: Romance scammers build trust over weeks before asking for money — be cautious with new online relationships.",
                    "⚠️ Tip: Government agencies will NEVER demand immediate payment via gift cards or cryptocurrency.",
                    "⚠️ Tip: Report scams to the South African Banking Risk Information Centre (SABRIC) at www.sabric.co.za.",
                },

                ["privacy tip"] = new List<string>
                {
                    "🔏 Tip: Review the privacy settings on all your social media accounts — limit what strangers can see.",
                    "🔏 Tip: Avoid oversharing personal details online — your full name, ID number, and address can enable identity theft.",
                    "🔏 Tip: Use a VPN when browsing on public networks to protect your data from eavesdroppers.",
                    "🔏 Tip: Regularly audit which apps have access to your camera, microphone, and location.",
                    "🔏 Tip: Use a private/incognito browsing mode when on shared computers.",
                },
            };
        }

        private Dictionary<string, string> BuildKeywordResponses()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["how are you"] = "I'm doing great — always on guard, just like a good firewall! How can I help you stay secure today?",
                ["hello"] = "Hello! I'm CyberBot, your cybersecurity awareness assistant. Ask me anything about staying safe online!",
                ["hi"] = "Hi there! Ready to boost your cybersecurity knowledge? Ask me about passwords, phishing, malware, and more!",

                ["what is your purpose"] = "My purpose is to raise cybersecurity awareness. I can help with passwords, phishing, safe browsing, malware, 2FA, privacy, encryption, and public Wi-Fi safety.",
                ["what do you do"] = "I educate users about cybersecurity risks and how to stay safe online. Try asking me about phishing, passwords, or scams!",
                ["help"] =
                    "Here are topics I can help you with:\n\n" +
                    "  🔑 Passwords        — 'tell me about password safety'\n" +
                    "  🎣 Phishing         — 'what is phishing?' or 'give me a phishing tip'\n" +
                    "  ⚠️ Scams            — 'I'm worried about online scams'\n" +
                    "  🔏 Privacy          — 'how do I protect my privacy?'\n" +
                    "  🦠 Malware          — 'what is malware?'\n" +
                    "  🔐 2FA              — 'what is two-factor authentication?'\n" +
                    "  🔒 Encryption       — 'explain encryption'\n" +
                    "  📶 Public Wi-Fi     — 'is public Wi-Fi safe?'\n" +
                    "  🌐 Safe Browsing    — 'how do I browse safely?'\n\n" +
                    "You can also say 'give me a phishing tip', 'give me a password tip', etc.!",

                ["password"] =
                    "🔑 PASSWORD SAFETY:\n\n" +
                    "• Use at least 12 characters — longer is always stronger.\n" +
                    "• Mix uppercase, lowercase, numbers, and symbols (e.g., @, #, !).\n" +
                    "• Never use your birthday, name, or 'password123'.\n" +
                    "• Use a unique password for EVERY account.\n" +
                    "• Consider a trusted Password Manager (Bitwarden, 1Password).\n" +
                    "• Enable 2FA wherever possible.\n\n" +
                    "✅ Strong: 'T!ger$Sunset#2025_Blue'\n" +
                    "❌ Weak: 'password123'\n\n" +
                    "Type 'give me a password tip' for more random tips!",

                ["phishing"] =
                    "🎣 PHISHING AWARENESS:\n\n" +
                    "Phishing is when attackers impersonate trusted organisations to steal\n" +
                    "your credentials via fake emails, SMS, or websites.\n\n" +
                    "HOW TO SPOT IT:\n" +
                    "• Urgent language: 'Your account will be closed!'\n" +
                    "• Suspicious sender: support@amaz0n-login.com\n" +
                    "• Unexpected attachments or suspicious links\n" +
                    "• Poor grammar or spelling\n" +
                    "• Requests for passwords or payment info\n\n" +
                    "WHAT TO DO:\n" +
                    "✔ Hover over links to check the real URL.\n" +
                    "✔ Report phishing to your email provider.\n" +
                    "✔ When in doubt — DELETE it!\n\n" +
                    "Type 'give me a phishing tip' for more random tips!",

                ["privacy"] =
                    "🔏 PROTECTING YOUR PRIVACY:\n\n" +
                    "• Limit what you share on social media (name, location, ID info).\n" +
                    "• Use strong, unique passwords on all accounts.\n" +
                    "• Enable 2FA on email and social media.\n" +
                    "• Use a VPN on public networks.\n" +
                    "• Review app permissions (camera, mic, location).\n" +
                    "• Use private browsing on shared computers.\n" +
                    "• Check if your email has been breached at: haveibeenpwned.com\n\n" +
                    "Type 'give me a privacy tip' for random tips!",

                ["scam"] =
                    "⚠️ AVOIDING ONLINE SCAMS:\n\n" +
                    "• Never give personal info to unsolicited callers or emailers.\n" +
                    "• Government agencies won't demand payment via gift cards.\n" +
                    "• Verify any 'urgent' messages through official channels.\n" +
                    "• If an offer seems too good to be true — it is.\n" +
                    "• Report scams to SAPS or www.cybercrime.gov.za.\n\n" +
                    "Type 'give me a scam tip' for more random tips!",

                ["malware"] =
                    "🦠 MALWARE & VIRUSES:\n\n" +
                    "Malware is malicious software that damages or gains unauthorised access.\n\n" +
                    "COMMON TYPES:\n" +
                    "• Virus       — attaches to files, spreads when executed.\n" +
                    "• Ransomware  — encrypts your files, demands payment.\n" +
                    "• Trojan      — disguises itself as legitimate software.\n" +
                    "• Spyware     — secretly monitors your activity.\n" +
                    "• Worm        — self-replicates across networks.\n\n" +
                    "PROTECTION:\n" +
                    "✔ Install and update a reputable antivirus.\n" +
                    "✔ Keep your OS and apps updated.\n" +
                    "✔ Don't click unknown links or open suspicious attachments.\n" +
                    "✔ Back up important files regularly.",

                ["virus"] =
                    "🦠 VIRUS PROTECTION:\n\n" +
                    "• Install a reputable antivirus (Windows Defender, Malwarebytes).\n" +
                    "• Keep it updated — new virus definitions are released daily.\n" +
                    "• Don't download software from untrusted sites.\n" +
                    "• Scan USB drives before opening files from them.",

                ["2fa"] =
                    "🔐 TWO-FACTOR AUTHENTICATION (2FA):\n\n" +
                    "2FA adds an extra security layer — even if your password is stolen,\n" +
                    "attackers still can't access your account without the second factor.\n\n" +
                    "2FA METHODS:\n" +
                    "• SMS code — one-time code sent to your phone.\n" +
                    "• Authenticator app — Google Authenticator, Authy (more secure than SMS).\n" +
                    "• Hardware key — YubiKey (most secure).\n" +
                    "• Biometrics — fingerprint / face recognition.\n\n" +
                    "✅ Enable 2FA on email, banking, and social media NOW!",

                ["two factor"] = "Two-factor authentication (2FA) adds a second login step beyond your password. It's one of the best defences against account takeovers!",
                ["multi-factor"] = "Multi-Factor Authentication (MFA) is similar to 2FA — it requires two or more forms of verification. Always enable it where available.",
                ["authentication"] = "Authentication is the process of verifying your identity. Use strong passwords + 2FA for best security.",

                ["encryption"] =
                    "🔒 ENCRYPTION EXPLAINED:\n\n" +
                    "Encryption converts readable data into an unreadable format. Only someone\n" +
                    "with the correct key can decrypt and read it.\n\n" +
                    "WHY IT MATTERS:\n" +
                    "• Protects data in transit (HTTPS uses TLS encryption).\n" +
                    "• Keeps files safe if your device is lost or stolen.\n" +
                    "• Used in WhatsApp, banking apps, email, and VPNs.\n\n" +
                    "GOOD PRACTICE:\n" +
                    "✔ Look for 'https://' before submitting any form.\n" +
                    "✔ Enable full-disk encryption (BitLocker on Windows).",

                ["safe browsing"] =
                    "🌐 SAFE BROWSING HABITS:\n\n" +
                    "• Always check for 'https://' and a padlock icon.\n" +
                    "• Avoid downloading files from untrusted sites.\n" +
                    "• Use a reputable, updated browser.\n" +
                    "• Install an ad-blocker (uBlock Origin).\n" +
                    "• Clear cookies and history regularly.\n" +
                    "• Never enter personal info on sites you don't trust.\n" +
                    "• Use a VPN on public networks.",

                ["browse safely"] = "To browse safely: always use HTTPS sites, avoid suspicious links, use an ad-blocker, and never enter sensitive details on unfamiliar websites.",
                ["internet safety"] = "Internet safety includes using strong passwords, avoiding suspicious links, enabling 2FA, keeping software updated, and being aware of phishing and scams.",

                ["wifi"] =
                    "📶 PUBLIC WI-FI SAFETY:\n\n" +
                    "Public Wi-Fi (cafes, airports) is often unsecured — attackers can intercept your traffic.\n\n" +
                    "RISKS:\n" +
                    "• Man-in-the-Middle (MitM) attacks.\n" +
                    "• Evil twin hotspots (fake networks mimicking legitimate ones).\n" +
                    "• Session hijacking.\n\n" +
                    "TIPS:\n" +
                    "✔ Use a VPN on public Wi-Fi.\n" +
                    "✔ Avoid banking/sensitive tasks on public networks.\n" +
                    "✔ Forget the network after use.\n" +
                    "✔ Prefer mobile data when possible.",

                ["wi-fi"] = "Public Wi-Fi is risky — always use a VPN and avoid accessing sensitive accounts on unsecured networks.",
                ["public wifi"] = "Never do online banking on public Wi-Fi. Use a VPN to encrypt your connection and protect your data.",

                ["social engineering"] =
                    "🕵️ SOCIAL ENGINEERING:\n\n" +
                    "Social engineering exploits human psychology — not software — to trick people\n" +
                    "into revealing confidential information.\n\n" +
                    "COMMON TACTICS:\n" +
                    "• Pretexting  — fabricating a scenario to extract info.\n" +
                    "• Baiting     — leaving infected USB drives in public.\n" +
                    "• Tailgating  — following someone into a secure area.\n" +
                    "• Vishing     — voice phishing via phone calls.\n\n" +
                    "HOW TO PROTECT YOURSELF:\n" +
                    "✔ Verify identities before sharing sensitive info.\n" +
                    "✔ Be sceptical of unexpected urgent requests.\n" +
                    "✔ Follow your organisation's security policies.",

                ["vpn"] =
                    "🛡️ VPN (Virtual Private Network):\n\n" +
                    "A VPN encrypts your internet connection and hides your IP address.\n\n" +
                    "BENEFITS:\n" +
                    "• Protects your data on public Wi-Fi.\n" +
                    "• Hides your browsing activity from your ISP.\n" +
                    "• Lets you access geo-restricted content safely.\n\n" +
                    "REPUTABLE VPNs: ProtonVPN (free tier), Mullvad, ExpressVPN.",
            };
        }

        private HashSet<string> BuildFollowUpSet()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "tell me more", "give me another tip", "more please",
                "explain more", "continue", "go on", "and then",
                "what else", "more info", "elaborate", "expand on that",
                "can you explain", "i want to know more", "more details",
            };
        }
    }
}