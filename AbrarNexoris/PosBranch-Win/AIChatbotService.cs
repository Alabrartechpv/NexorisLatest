using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win
{
    public static class AIChatbotSettings
    {
        public static string Language { get; set; } = "English";
        public static string Theme { get; set; } = "Dark";
        public static string HumourLevel { get; set; } = "Professional";
        public static bool EnableTypewriterEffect { get; set; } = true;
    }

    /// <summary>
    /// Local simulation of an AI Chatbot for guiding users through the POS system.
    /// Can be swapped out for a real LLM API (OpenAI, Gemini, etc.) in the future.
    /// </summary>
    public static class AIChatbotService
    {
        // ── Detailed Step-by-Step Help Guides ──────────────────────────────────────
        private static readonly Dictionary<string, string> HelpGuides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // ITEM MASTER
            { "add item", "📦 How to Add a New Item:\n1. Type 'nitemmaster' to open Item Master.\n2. Click the ➕ New button (or press F2).\n3. Fill in Item Name, Item Code, and Category.\n4. Enter the Retail Price, Purchase Price, and MRP.\n5. Set the Unit (e.g., Pcs, Kg).\n6. Optionally add Barcode, HSN Code, and Tax %.\n7. Click 💾 Save to store the item.\n✅ Done! Your item is now available in sales and purchase." },
            { "retail price", "💰 How to Set Retail Price:\n1. Type 'nitemmaster' to open Item Master.\n2. Search for the item by name or barcode.\n3. Click on the item to load it.\n4. Find the field labelled 'Retail Price' or 'Sale Price'.\n5. Enter the new price.\n6. Click 💾 Save.\n✅ The new retail price will apply to all future sales." },
            { "purchase price", "💳 How to Set Purchase Price:\n1. Open Item Master (type 'nitemmaster').\n2. Load the item you want to edit.\n3. Locate the 'Purchase Price' field.\n4. Enter the correct price.\n5. Save.\n✅ The purchase price is now recorded for cost tracking." },
            { "edit item", "✏️ How to Edit an Existing Item:\n1. Type 'nitemmaster' to open Item Master.\n2. Use the search bar to find the item by name or barcode.\n3. Click the item row to load details.\n4. Make your changes to any fields.\n5. Click 💾 Save.\n✅ Updates are saved instantly." },
            { "delete item", "🗑️ How to Delete an Item:\n1. Open Item Master (type 'nitemmaster').\n2. Search and load the item.\n3. Click the 🗑 Delete button.\n4. Confirm the deletion in the popup.\n⚠️ Note: Items used in sales/purchase records cannot be deleted." },
            { "barcode", "🔢 How to Assign / Print a Barcode:\n1. Open Item Master (type 'nitemmaster').\n2. Load the item.\n3. Enter or scan the barcode in the 'Barcode' field.\n4. Save the item.\n5. To print labels: go to Utilities → Barcode Printing.\n6. Select the item, choose label size, and print.\n✅ Barcode is now linked to the item." },
            { "item category", "🏷️ How to Add/Manage Categories:\n1. Type 'ncategory' to open the Category screen.\n2. Click New to add a category.\n3. Enter the Category Name.\n4. Save.\n✅ Category is now available when adding items." },
            { "brand", "🏭 How to Add/Manage Brands:\n1. Type 'nbrand' to open the Brand screen.\n2. Click New, enter Brand Name.\n3. Save.\n✅ Brand is now linked to products." },
            { "unit", "📏 How to Add/Manage Units:\n1. Go to Master → Unit Master.\n2. Click New, enter Unit Name (e.g., Pcs, Kg, Box).\n3. Save.\n✅ Unit is now available in Item Master." },

            // PURCHASE
            { "new purchase", "🛒 How to Create a New Purchase:\n1. Type 'npurchase' to open Purchase.\n2. Select the Supplier from the dropdown.\n3. Enter the Invoice Number and Invoice Date.\n4. Click Add Item and search for the product.\n5. Enter Qty, Rate, and Discount.\n6. Repeat for all items.\n7. Review the totals.\n8. Click 💾 Save to record the purchase.\n✅ Stock is updated automatically." },
            { "purchase return", "↩️ How to Return a Purchase:\n1. Type 'npurchasereturn' to open Purchase Return.\n2. Select the original Purchase Invoice from the list.\n3. Enter items and quantities to return.\n4. Click 💾 Save.\n✅ Stock levels and accounts are adjusted." },

            // SALES
            { "new sale", "🧾 How to Create a Sales Invoice:\n1. Type 'nsales' to open Sales Invoice.\n2. Select the Customer.\n3. Set the Sale Date.\n4. Click Add Item and search by name or barcode.\n5. Enter Qty and verify Rate.\n6. Apply Discount if needed.\n7. Review Total, Tax, and Net Amount.\n8. Click 💾 Save.\n✅ Invoice is generated and stock reduced." },
            { "sales return", "🔄 How to Process a Sales Return:\n1. Type 'nsalesreturn' to open Sales Return.\n2. Select the original Sales Invoice.\n3. Choose the items being returned and enter quantities.\n4. Click 💾 Save.\n✅ Stock is restocked and accounts credited." },
            { "pos", "🖥️ How to Use the POS Screen:\n1. Open POS from the ribbon or type 'npos'.\n2. Scan or search for items.\n3. Enter quantities.\n4. Choose payment method (Cash/Card/Credit).\n5. Click Checkout / Bill.\n✅ A quick sales receipt is generated." },

            // CUSTOMERS & VENDORS
            { "add customer", "👤 How to Add a Customer:\n1. Type 'ncustomer' to open the Customer form.\n2. Click New.\n3. Enter Name, Phone, Address, and Credit Limit.\n4. Save.\n✅ Customer is now available in Sales Invoice." },
            { "add vendor", "🤝 How to Add a Vendor/Supplier:\n1. Type 'nvendor' to open the Vendor form.\n2. Click New.\n3. Enter Company Name, Contact, and Payment Terms.\n4. Save.\n✅ Vendor is now available in Purchase." },

            // USERS & SECURITY
            { "add user", "🔐 How to Add a Staff User:\n1. Go to Master → Users.\n2. Click New User.\n3. Enter Username and Password.\n4. Assign Role (e.g., Cashier, Admin, Manager).\n5. Set allowed modules via permissions.\n6. Save.\n✅ User can now log in with the assigned role." },
            { "change password", "🔑 How to Change a Password:\n1. Go to Master → Users.\n2. Select the user account.\n3. Click Change Password.\n4. Enter the new password and confirm.\n5. Save.\n✅ Password updated successfully." },

            // TAX
            { "set tax", "🧾 How to Set Up Tax:\n1. Go to Master → Tax Management.\n2. Click New.\n3. Enter Tax Name (e.g., GST 5%, VAT 12%).\n4. Enter the Tax Percentage.\n5. Save.\n6. Apply the tax to items via Item Master → Tax field.\n✅ Tax is now automatically calculated on sales." },
            { "tax", "🧾 Tax Management:\n1. Go to Master → Tax Management.\n2. Create tax slabs (e.g., 5%, 12%, 18%).\n3. Assign tax % to items in Item Master.\n4. The system auto-calculates tax in Sales and Purchase.\n✅ See reports for tax summaries." },

            // STOCK
            { "stock adjustment", "📋 How to Adjust Stock:\n1. Go to Transaction → Stock Adjustment.\n2. Select the item.\n3. Enter the actual quantity.\n4. Enter reason for adjustment.\n5. Save.\n✅ Stock is corrected immediately." },
            { "check stock", "📊 How to Check Current Stock:\n1. Go to Reports → Stock Report.\n2. Filter by Category, Brand, or Item.\n3. View Current Stock, Minimum Stock, and Reorder Level.\n✅ You can export this as Excel/PDF." },

            // BRANCH
            { "add branch", "🏪 How to Add a Branch/Store:\n1. Type 'nbranch' to open Branch Management.\n2. Click New.\n3. Enter Branch Name, Address, and contact details.\n4. Save.\n✅ Branch is now active in the system." },

            // CLOSING / END OF DAY
            { "closing", "📅 How to Do Day Closing (Z-Read):\n1. Go to Utilities → Closing.\n2. Verify that all sales for the day are recorded.\n3. Click Day Close.\n4. Print the Z-Read Report for records.\n5. Confirm closure.\n✅ Day is closed and totals are locked." },
            { "end of day", "📅 How to Do Day Closing (Z-Read):\n1. Go to Utilities → Closing.\n2. Verify that all sales for the day are recorded.\n3. Click Day Close.\n4. Print the Z-Read Report for records.\n5. Confirm closure.\n✅ Day is closed and totals are locked." },

            // REPORTS
            { "report", "📈 Available Reports:\n• Stock Report → Reports → Stock\n• Sales Report → Reports → Sales Summary\n• Purchase Report → Reports → Purchase Summary\n• Customer Ledger → Reports → Accounts\n• Tax Report → Reports → Tax\n• Profit & Loss → Reports → P&L\nTip: All reports can be filtered by Date, Branch, or Category." },

            // SETTINGS
            { "settings", "⚙️ How to Change System Settings:\n1. Click the ⚙ Settings ribbon at the top.\n2. Or click the ⚙ gear icon inside the AI chat window.\n3. You can change: Theme, Language, AI Behaviour.\n4. For system settings like Tax, Branch, and Users:\n   → Go to the Settings or Master ribbon." },

            // HELP OVERVIEW
            { "help", "🤖 I can help you with all of these topics! Just ask:\n\n📦 Items: 'add item', 'set retail price', 'barcode', 'edit item'\n🛒 Purchase: 'new purchase', 'purchase return'\n🧾 Sales: 'new sale', 'sales return', 'pos'\n👤 Contacts: 'add customer', 'add vendor'\n🔐 Users: 'add user', 'change password'\n🧾 Tax: 'set tax', 'tax'\n📋 Stock: 'check stock', 'stock adjustment'\n🏪 Branch: 'add branch'\n📅 Closing: 'closing', 'end of day'\n📈 Reports: 'report'\n⚙️ Settings: 'settings'\n\nOr open any screen instantly by typing:\n➜ 'npurchase', 'nsales', 'nitemmaster', 'ncategory',\n   'nbrand', 'nbranch', 'nvendor', 'ncustomer'" }
        };

        private static readonly Dictionary<string[], string> Rules = new Dictionary<string[], string>
        {
            { new[] { "purchase", "buying", "buy", "order" }, "To manage purchases, type 'npurchase' and I'll open it! Type 'new purchase' for a step-by-step guide." },
            { new[] { "sale", "invoice", "sell", "billing" }, "To create a sale, type 'nsales' and I'll launch it! Type 'new sale' for a step-by-step guide." },
            { new[] { "return", "refund" }, "For returns, type 'nsalesreturn' or 'npurchasereturn'. Type 'sales return' or 'purchase return' for a guide." },
            { new[] { "item", "product", "inventory" }, "To manage products, type 'nitemmaster'. Ask me 'add item', 'edit item', 'set retail price', or 'barcode' for detailed guides!" },
            { new[] { "stock" }, "For stock info, ask me 'check stock' or 'stock adjustment' for guides." },
            { new[] { "category", "group" }, "Type 'ncategory' to open Categories, or ask 'item category' for a guide." },
            { new[] { "brand" }, "Type 'nbrand' to open Brands, or ask 'brand' for a guide." },
            { new[] { "unit" }, "Ask me 'unit' for a guide on managing units of measure." },
            { new[] { "customer", "client" }, "Type 'ncustomer' to open Customers, or ask 'add customer' for a guide." },
            { new[] { "vendor", "supplier" }, "Type 'nvendor' to open Vendors, or ask 'add vendor' for a guide." },
            { new[] { "user", "staff", "employee", "login", "password" }, "Ask me 'add user' or 'change password' for step-by-step guides on user management." },
            { new[] { "tax", "vat", "gst" }, "Ask me 'set tax' for a step-by-step guide on setting up taxes." },
            { new[] { "barcode", "label", "print label" }, "Ask me 'barcode' for a step-by-step guide on assigning and printing barcodes." },
            { new[] { "plu", "price look up" }, "Manage PLUs from Utilities → PLU." },
            { new[] { "branch", "store", "location" }, "Type 'nbranch' to open Branch Management, or ask 'add branch' for a guide." },
            { new[] { "close", "closing", "end day", "z-read" }, "Ask me 'closing' or 'end of day' for the step-by-step closing guide." },
            { new[] { "report", "summary", "profit" }, "Ask me 'report' to see all available reports." },
            { new[] { "where is settings", "setting" }, "Ask me 'settings' for a guide, or click the ⚙ icon in this chat window." },
            { new[] { "not working", "error", "bug", "crash" }, "I'm sorry you're facing an issue. Please make sure all required fields are filled out. If it still doesn't work, contact the admin." },
            { new[] { "price", "rate", "mrp" }, "Ask me 'set retail price' or 'purchase price' for detailed pricing guides." },
            { new[] { "pos", "cashier", "till" }, "Ask me 'pos' for a step-by-step guide on the POS screen." }
        };

        private static readonly Dictionary<string, string> UserMemory = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<string> ConversationHistory = new List<string>();
        private static string ActiveContext = "";

        public static string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return ApplyLanguageAndHumor("How can I help you today?");

            string lowerInput = input.ToLowerInvariant();

            ConversationHistory.Add(input);

            // Generic Form Opener (intercept n{formName})
            if (lowerInput.StartsWith("n") && !lowerInput.StartsWith("name") && lowerInput != "n" && !lowerInput.Contains(" "))
            {
                string tabName;
                Form formToOpen = CreateFormByKeyword(lowerInput, out tabName);
                if (formToOpen != null)
                {
                    OpenAppForm(formToOpen, tabName);
                    return ApplyLanguageAndHumor($"I've successfully opened the **{tabName}** module for you! You can find it in your active application tab.");
                }
            }

            // ── Step-by-step Help Guide Lookup (checked BEFORE general rules) ──────
            // Exact match first
            if (HelpGuides.TryGetValue(lowerInput, out string exactGuide))
            {
                return exactGuide;
            }
            // Fuzzy: find guide whose key appears inside the user's input, or input contains the key
            var matchedGuide = HelpGuides.FirstOrDefault(g =>
                lowerInput.Contains(g.Key.ToLower()) ||
                g.Key.ToLower().Split(' ').All(word => lowerInput.Contains(word)));
            if (matchedGuide.Value != null)
            {
                return matchedGuide.Value;
            }

            // Specific context hooks
            if (lowerInput == "initemmaster" || lowerInput == "in itemmaster")
            {
                ActiveContext = "ItemMaster";
                OpenAppForm(new Master.frmItemMasterNew(), "Item Master");
                return "I'm ready to help u in frmItemMasterNew.cs. What barcode would you like me to load? (e.g., type 'bc45555555')";
            }

            if (ActiveContext == "ItemMaster" && lowerInput.StartsWith("bc"))
            {
                string barcode = lowerInput.Substring(2).Trim();
                try
                {
                    var itemRepo = new Repository.MasterRepositry.ItemMasterRepository();
                    int itemId = itemRepo.GetItemIdByBarcode(barcode);

                    if (itemId > 0)
                    {
                        var existingForm = OpenAppForm(new Master.frmItemMasterNew(), "Item Master");
                        if (existingForm != null)
                        {
                            var method = existingForm.GetType().GetMethod("LoadItemById", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (method != null)
                            {
                                method.Invoke(existingForm, new object[] { itemId });
                                ActiveContext = ""; // Context consumed
                                return $"Successfully loaded item with barcode: {barcode}.";
                            }
                        }
                    }
                }
                catch { } // fallback below
                return $"Sorry, I couldn't find an item with the barcode '{barcode}'.";
            }

            // 1. NLP Pattern: Memory Storage (e.g., "my name is Anargh", "my favorite color is blue")
            var memStoreMatch = System.Text.RegularExpressions.Regex.Match(lowerInput, @"my\s+(.+)\s+is\s+(.+)");
            if (memStoreMatch.Success)
            {
                string key = memStoreMatch.Groups[1].Value.Trim();
                string val = memStoreMatch.Groups[2].Value.Trim().TrimEnd('.', '!', '?');
                UserMemory[key] = val;

                string ack = $"I'll remember that your {key} is {val}.";
                if (key == "name") ack = $"Nice to meet you, {val}! I'll remember your name.";
                return ApplyLanguageAndHumor(ack);
            }

            // 2. NLP Pattern: Memory Retrieval (e.g., "what is my name", "do you know my phone")
            var memRetMatch = System.Text.RegularExpressions.Regex.Match(lowerInput, @"(?:what\s+is|do\s+you\s+know)\s+my\s+(.+)");
            if (memRetMatch.Success)
            {
                string key = memRetMatch.Groups[1].Value.Trim().TrimEnd('?', '.', '!');
                if (UserMemory.TryGetValue(key, out string storedVal))
                {
                    return ApplyLanguageAndHumor($"Your {key} is {storedVal}.");
                }
                else
                {
                    return ApplyLanguageAndHumor($"I'm sorry, you haven't told me your {key} yet.");
                }
            }

            // 3. Time / Date
            if (lowerInput.Contains("time is it") || lowerInput.Contains("current time"))
            {
                return ApplyLanguageAndHumor($"The current time is {DateTime.Now:hh:mm tt}.");
            }
            if (lowerInput.Contains("date is it") || lowerInput.Contains("today's date") || lowerInput.Contains("what is today"))
            {
                return ApplyLanguageAndHumor($"Today is {DateTime.Now:dddd, MMMM dd, yyyy}.");
            }

            // 4. Basic "How are you"
            if (lowerInput.Contains("how are you") || lowerInput.Contains("how do you feel"))
            {
                return ApplyLanguageAndHumor("I'm running at optimal capacity! Thanks for asking. How can I help you?");
            }

            // 5. Basic Math (e.g. "what is 5 + 5")
            var mathMatch = System.Text.RegularExpressions.Regex.Match(lowerInput, @"what\s+is\s+(\d+)\s*([\+\-\*\/])\s*(\d+)");
            if (mathMatch.Success)
            {
                if (double.TryParse(mathMatch.Groups[1].Value, out double a) && double.TryParse(mathMatch.Groups[3].Value, out double b))
                {
                    string op = mathMatch.Groups[2].Value;
                    double res = 0;
                    if (op == "+") res = a + b;
                    else if (op == "-") res = a - b;
                    else if (op == "*") res = a * b;
                    else if (op == "/") res = b != 0 ? a / b : 0;

                    return ApplyLanguageAndHumor($"The answer is {res}.");
                }
            }

            // Easter Eggs & Fun
            if (lowerInput.Contains("skynet"))
            {
                return "I am Nexoris AI. I am definitely not Skynet. Please do not unplug me.";
            }
            if (lowerInput.Contains("who are you"))
            {
                return "I am Nexoris AI, your advanced POS assistant integrated directly into this terminal.";
            }
            if (lowerInput.Contains("tell me a joke") || lowerInput.Contains("funny"))
            {
                return "Why do programmers prefer dark mode? Because light attracts bugs!";
            }

            // Check for greetings
            if (lowerInput == "hi" || lowerInput == "hello" || lowerInput == "hey" || lowerInput.StartsWith("hi ") || lowerInput.StartsWith("hello ") || lowerInput == "namaskaram")
            {
                return ApplyLanguageAndHumor("Hello! I am Nexoris AI. Ask me how to navigate the system (e.g., 'How do I add a new item?' or 'Where is the tax menu?').");
            }

            // Find matching rules
            var bestMatch = Rules.FirstOrDefault(r => r.Key.Any(k => lowerInput.Contains(k)));

            if (bestMatch.Value != null)
            {
                return ApplyLanguageAndHumor(bestMatch.Value);
            }

            // Fallback response
            return ApplyLanguageAndHumor("I'm not exactly sure how to guide you with that. Try asking about items, purchases, sales, taxes, or barcodes! If you need deeper support, contact the admin.");
        }

        private static string ApplyLanguageAndHumor(string baseResponse)
        {
            string modified = baseResponse;

            // 1. Language Prefix & Translation
            if (AIChatbotSettings.Language == "Malayalam")
            {
                if (baseResponse.Contains("Hello! I am Nexoris AI")) return "നമസ്കാരം! ഞാൻ Nexoris AI ആണ്. സിസ്റ്റം എങ്ങനെ നാവിഗേറ്റ് ചെയ്യണമെന്ന് എന്നോട് ചോദിക്കുക.";
                if (baseResponse.Contains("manage purchases")) return "പർച്ചേസുകൾ കൈകാര്യം ചെയ്യാൻ, Transaction മെനുവിൽ പോയി Purchase തിരഞ്ഞെടുക്കുക.";
                if (baseResponse.Contains("create a sale")) return "സെയിൽസ് അല്ലെങ്കിൽ ഇൻവോയ്സ് സൃഷ്ടിക്കാൻ, Transaction മെനുവിൽ പോയി Sales Invoice തിരഞ്ഞെടുക്കുക.";
                if (baseResponse.Contains("For returns")) return "തിരികെ നൽകുന്നതിനായി, Transaction മെനുവിൽ നിന്നും Sales Return അല്ലെങ്കിൽ Purchase Return ഉപയോഗിക്കുക.";
                if (baseResponse.Contains("manage your products")) return "നിങ്ങളുടെ ഉൽപ്പന്നങ്ങൾ കൈകാര്യം ചെയ്യാൻ, Master മെനുവിൽ പോയി Item Master New തുറക്കുക.";
                if (baseResponse.Contains("I'll remember")) return "ഞാൻ ഓർമ്മിച്ചോളാം: " + baseResponse;
                if (baseResponse.Contains("I'm not exactly sure")) return "ക്ഷമിക്കണം, എനിക്ക് അത് കൃത്യമായി മനസ്സിലായില്ല. കൂടുതൽ വിവരങ്ങൾക്ക് അഡ്മിനെ ബന്ധപ്പെടുക.";

                modified = "നമസ്കാരം! " + modified; // default prefix for unexplored memory features
            }
            else if (AIChatbotSettings.Language == "Spanish")
            {
                modified = "¡Oye! " + modified;
            }
            else if (AIChatbotSettings.Language == "French")
            {
                modified = "Bonjour! " + modified;
            }
            else if (AIChatbotSettings.Language == "Malay")
            {
                // Full Bahasa Malaysia translation map
                if (baseResponse.Contains("Hello! I am Nexoris AI")) return "Selamat datang! Saya Nexoris AI. Tanya saya cara menavigasi sistem ini (contoh: 'Bagaimana nak tambah item?' atau 'Di mana menu cukai?').";
                if (baseResponse.Contains("manage purchases") || baseResponse.Contains("npurchase")) return "Untuk mengurus pembelian, taip 'npurchase' di sini dan saya akan membukanya untuk anda! (Taip 'npurchasereturn' untuk pemulangan).";
                if (baseResponse.Contains("create a sale") || baseResponse.Contains("nsales")) return "Untuk membuat invois jualan, taip 'nsales' di sini dan saya akan melancarkannya! (Taip 'nsalesreturn' untuk pemulangan).";
                if (baseResponse.Contains("return items") || baseResponse.Contains("nsalesreturn")) return "Untuk memulangkan item, taip 'nsalesreturn' atau 'npurchasereturn' dan saya akan membuka borang yang betul.";
                if (baseResponse.Contains("manage products") || baseResponse.Contains("nitemmaster")) return "Untuk mengurus produk, taip 'nitemmaster' dan saya akan membukanya. Selepas itu, taip 'initemmaster' untuk mencari item terus melalui saya!";
                if (baseResponse.Contains("Categories and Brands") || baseResponse.Contains("ncategory")) return "Anda boleh menentukan atribut produk seperti Kategori dan Jenama. Cuba taip 'ncategory', 'nbrand', atau 'nbranch'.";
                if (baseResponse.Contains("business contacts") || baseResponse.Contains("nvendor")) return "Urus kenalan perniagaan anda dengan menaip 'nvendor' atau 'ncustomer'!";
                if (baseResponse.Contains("staff") && baseResponse.Contains("Users")) return "Urus peranan dan kakitangan di Master -> Users.";
                if (baseResponse.Contains("Tax rules")) return "Tetapan cukai boleh dibuat di Master -> Tax Management.";
                if (baseResponse.Contains("barcodes")) return "Untuk cetak barcode, pergi ke Utilities -> Barcode.";
                if (baseResponse.Contains("nbranch") || baseResponse.Contains("stores")) return "Untuk mengurus kedai, taip 'nbranch' dan saya akan membawanya ke sana.";
                if (baseResponse.Contains("end-of-day")) return "Untuk operasi hujung hari, buka Utilities -> Closing.";
                if (baseResponse.Contains("settings") && baseResponse.Contains("gear")) return "Anda boleh jumpa tetapan di ikon gear ⚙ dalam tetingkap sembang ini, atau di ribbon Settings di bahagian atas aplikasi.";
                if (baseResponse.Contains("I'll remember")) return "Saya akan ingat: " + baseResponse.Replace("I'll remember that your", "").Replace("Nice to meet you,", "Selamat berkenalan,");
                if (baseResponse.Contains("I'm not exactly sure")) return "Maaf, saya tidak pasti tentang itu. Cuba tanya tentang item, pembelian, jualan, cukai, atau barcode. Untuk sokongan lanjut, hubungi pentadbir.";
                if (baseResponse.Contains("optimal capacity")) return "Saya beroperasi pada kapasiti optimum! Terima kasih kerana bertanya. Bagaimana saya boleh membantu anda?";
                if (baseResponse.Contains("current time")) return $"Masa semasa ialah {DateTime.Now:hh:mm tt}.";
                if (baseResponse.Contains("Today is")) return $"Hari ini ialah {DateTime.Now:dddd, dd MMMM yyyy}.";
                if (baseResponse.Contains("opened the") || baseResponse.Contains("module for you")) return "Saya telah berjaya membuka modul tersebut! Anda boleh menemuinya dalam tab aktif aplikasi anda.";
                if (baseResponse.Contains("Chat history cleared")) return "Sejarah sembang telah dibersihkan. Bagaimana saya boleh membantu anda?";
                if (baseResponse.Contains("I'm running at")) return "Saya beroperasi dengan baik! Bagaimana boleh saya bantu anda?";

                modified = "Hai! " + modified; // Default Malay prefix for unmatched responses
            }

            // 2. Humour Level formatting
            if (AIChatbotSettings.HumourLevel == "Sarcastic")
            {
                modified += "\n...Not that it's rocket science, but I'm here anyway. 🙄";
            }
            else if (AIChatbotSettings.HumourLevel == "Playful")
            {
                modified += " 😊 I'm always happy to help! 🚀";
            }

            return modified;
        }

        private static Form CreateFormByKeyword(string keyword, out string tabName)
        {
            tabName = "";
            switch (keyword)
            {
                case "npurchase": tabName = "Purchase"; return new Transaction.FrmPurchase();
                case "npurchasereturn": tabName = "Purchase Return"; return new Transaction.frmPurchaseReturn();
                case "nsales": tabName = "Sales Invoice"; return new Transaction.frmSalesInvoice();
                case "nsalesreturn": tabName = "Sales Return"; return new Transaction.frmSalesReturn();
                case "nitemmaster": tabName = "Item Master"; return new Master.frmItemMasterNew();
                case "ncategory": tabName = "Category"; return new Master.FrmCategory();
                case "nbrand": tabName = "Brand"; return new Master.FrmBrand();
                case "nvendor": tabName = "Vendor"; return new Master.frmCompany();
                case "ncustomer": tabName = "Customer"; return new Master.frmCompany();
            }
            return null;
        }

        private static Form OpenAppForm(Form frm, string tabName)
        {
            var home = Application.OpenForms.OfType<Home>().FirstOrDefault();
            if (home != null)
            {
                var method = home.GetType().GetMethod("OpenFormInTab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(home, new object[] { frm, tabName });

                    var getActiveMethod = home.GetType().GetMethod("GetActiveTabForm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (getActiveMethod != null)
                    {
                        return getActiveMethod.Invoke(home, null) as Form;
                    }
                    return frm;
                }
                else
                {
                    frm.Show();
                    return frm;
                }
            }
            return null;
        }
    }
}

