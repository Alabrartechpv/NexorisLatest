using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinToolbars;

namespace PosBranch_Win
{
    public class LanguageItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string FlagSymbol { get; set; }
        public bool IsCustom { get; set; }
        public string FilePath { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Code.ToUpper()})";
        }
    }

    public static class LanguageManager
    {
        public static event EventHandler LanguageChanged;

        private static string _currentLanguageCode = "en";
        private static readonly Dictionary<string, LanguageItem> _availableLanguages = new Dictionary<string, LanguageItem>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, string> _originalTextMap = new System.Runtime.CompilerServices.ConditionalWeakTable<object, string>();

        private static readonly string ConfigDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
        private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "language_config.json");
        private static readonly string LanguagesDirectory = Path.Combine(ConfigDirectory, "Languages");

        static LanguageManager()
        {
            InitializeBuiltInLanguages();
            EnsureDirectoriesExist();
            LoadCustomLanguages();
            LoadSavedConfiguration();
        }

        public static string CurrentLanguageCode => _currentLanguageCode;

        public static string CurrentLanguageName
        {
            get
            {
                if (_availableLanguages.TryGetValue(_currentLanguageCode, out var item))
                    return item.Name;
                return "English";
            }
        }

        public static List<LanguageItem> GetAvailableLanguages()
        {
            return _availableLanguages.Values.ToList();
        }

        private static void EnsureDirectoriesExist()
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                    Directory.CreateDirectory(ConfigDirectory);
                if (!Directory.Exists(LanguagesDirectory))
                    Directory.CreateDirectory(LanguagesDirectory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating language directories: {ex.Message}");
            }
        }

        private static void InitializeBuiltInLanguages()
        {
            // 1. ENGLISH (en)
            _availableLanguages["en"] = new LanguageItem { Code = "en", Name = "English", FlagSymbol = "🇬🇧", IsCustom = false };
            var enDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Home", "Home" }, { "Master", "Master" }, { "Transaction", "Transaction" }, { "Accounts", "Accounts" },
                { "Vendor", "Vendor" }, { "Reports", "Reports" }, { "Utilities", "Utilities" }, { "Settings", "Settings" },
                { "Manual Balance", "Manual Balance" }, { "Company", "Company" }, { "Branch", "Branch" }, { "State", "State" },
                { "Country", "Country" }, { "Currency", "Currency" }, { "Group", "Group" }, { "Category", "Category" },
                { "ItemMaster", "Item Master" }, { "Item Master", "Item Master" }, { "Brand", "Brand" }, { "Users", "Users" },
                { "Reason", "Reason" }, { "Sales", "Sales" }, { "Sales Details", "Sales Details" }, { "Sales Return", "Sales Return" },
                { "Purchase", "Purchase" }, { "Purchase Order", "Purchase Order" }, { "Purchase Return", "Purchase Return" },
                { "Stock Adjustment", "Stock Adjustment" }, { "Stock Transfer", "Stock Transfer" }, { "Customer", "Customer" },
                { "Ledger", "Ledger" }, { "Receipt", "Receipt" }, { "Payment", "Payment" }, { "General Payment", "General Payment" },
                { "General Receipt", "General Receipt" }, { "Contra", "Contra" }, { "Journal", "Journal" }, { "DebitNote", "Debit Note" },
                { "Debit Note", "Debit Note" }, { "CreditNote", "Credit Note" }, { "Credit Note", "Credit Note" }, { "ChartOfAccount", "Chart of Accounts" },
                { "Print Barcode", "Print Barcode" }, { "PLU Weighing", "PLU Weighing" }, { "OpeningStock", "Opening Stock" },
                { "Opening Stock", "Opening Stock" }, { "Closing", "Closing" }, { "Database Maintenance", "Database Maintenance" },
                { "Sale Settings", "Sale Settings" }, { "Excel Import/Export", "Excel Import/Export" }, { "Import/Export", "Import/Export" },
                { "Roles", "Roles" }, { "RolePermission", "Role Permission" }, { "Role Permissions", "Role Permission" },
                { "Tax Management", "Tax Management" }, { "TaxManagement", "Tax Management" }, { "ActivityLog", "Activity Log" },
                { "Activity Log", "Activity Log" }, { "Year Closing", "Year Closing" }, { "Financial Year Closing", "Year Closing" },
                { "App Language", "App Language" }, { "Language Settings", "Language Settings" }, { "Save", "Save" }, { "Clear", "Clear" },
                { "Delete", "Delete" }, { "Update", "Update" }, { "Cancel", "Cancel" }, { "Print", "Print" }, { "Close", "Close" },
                { "Search", "Search" }, { "Filter", "Filter" }, { "Refresh", "Refresh" }, { "Hold", "Hold" }, { "Last Bill", "Last Bill" },
                { "Export", "Export" }, { "Import", "Import" }, { "Bill No", "Bill No" }, { "Customer Name", "Customer Name" },
                { "Item Name", "Item Name" }, { "Barcode", "Barcode" }, { "Unit", "Unit" }, { "Qty", "Qty" }, { "Quantity", "Quantity" },
                { "Unit Price", "Unit Price" }, { "Price", "Price" }, { "Total Amount", "Total Amount" }, { "Amount", "Amount" },
                { "Net Amount", "Net Amount" }, { "Cost", "Cost" }, { "Selling Price", "Selling Price" }, { "Billed By", "Billed By" },
                { "Date & Time", "Date & Time" }, { "Date", "Date" }, { "Payment Mode", "Payment Mode" }, { "Paymode", "Paymode" },
                { "Total Sales", "Total Sales" }, { "Total Orders", "Total Orders" }, { "Average Order", "Average Order" },
                { "Total Profit", "Total Profit" }, { "Items Sold", "Items Sold" }, { "Hold Item", "Hold Item" }, { "Ready", "Ready" },
                { "Success", "Success" }, { "Error", "Error" }, { "Warning", "Warning" }, { "Information", "Information" },
                { "Select Language", "Select Language" }, { "Import Custom Language", "Import Custom Language" },
                { "Export Template", "Export Template" }, { "Apply Language", "Apply Language" },
                { "Reset to Default (English)", "Reset to Default (English)" },
                { "Language changed successfully to", "Language changed successfully to" },
                { "Rolled back to Default Language (English)", "Rolled back to Default Language (English)" }
            };
            _translations["en"] = enDict;

            // 2. HINDI (hi)
            _availableLanguages["hi"] = new LanguageItem { Code = "hi", Name = "Hindi (हिन्दी)", FlagSymbol = "🇮🇳", IsCustom = false };
            var hiDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Home", "मुख्य पृष्ठ" }, { "Master", "मास्टर" }, { "Transaction", "लेन-देन" }, { "Accounts", "खाते" },
                { "Vendor", "विक्रेता" }, { "Reports", "रिपोर्ट्स" }, { "Utilities", "उपयोगिताएँ" }, { "Settings", "सेटिंग्स" },
                { "Manual Balance", "मैनुअल बैलेंस" }, { "Sale Settings", "बिक्री सेटिंग्स" }, { "Import/Export", "आयात/निर्यात" },
                { "Roles", "भूमिकाएं" }, { "RolePermission", "अनुमति प्रबंधन" }, { "Tax Management", "कर प्रबंधन" },
                { "TaxManagement", "कर प्रबंधन" }, { "ActivityLog", "गतिविधि लॉग" }, { "Activity Log", "गतिविधि लॉग" },
                { "Year Closing", "वर्ष समाप्ति" }, { "Financial Year Closing", "वित्तीय वर्ष समाप्ति" }, { "App Language", "ऐप भाषा" },
                { "Language Settings", "भाषा सेटिंग्स" }, { "Save", "सहेजें" }, { "Clear", "साफ करें" }, { "Delete", "हटाएं" },
                { "Update", "अद्यतन करें" }, { "Cancel", "रद्द करें" }, { "Print", "प्रिंट" }, { "Close", "बंद करें" }, { "Search", "खोजें" },
                { "Filter", "फ़िल्टर" }, { "Refresh", "ताज़ा करें" }, { "Hold", "होल्ड करें" }, { "Last Bill", "अंतिम बिल" },
                { "Export", "निर्यात" }, { "Import", "आयात" }, { "Bill No", "बिल संख्या" }, { "Customer Name", "ग्राहक का नाम" },
                { "Customer", "ग्राहक" }, { "Item Name", "वस्तु का नाम" }, { "Barcode", "बारकोड" }, { "Unit", "इकाई" },
                { "Qty", "मात्रा" }, { "Unit Price", "इकाई मूल्य" }, { "Total Amount", "कुल राशि" }, { "Amount", "राशि" },
                { "Net Amount", "शुद्ध राशि" }, { "Cost", "लागत" }, { "Selling Price", "बिक्री मूल्य" }, { "Billed By", "बिलकर्ता" },
                { "Date & Time", "दिनांक एवं समय" }, { "Date", "दिनांक" }, { "Payment Mode", "भुगतान का प्रकार" }, { "Total Sales", "कुल बिक्री" },
                { "Total Orders", "कुल ऑर्डर" }, { "Average Order", "औसत ऑर्डर" }, { "Total Profit", "कुल लाभ" },
                { "Items Sold", "बेची गई वस्तुएं" }, { "Hold Item", "होल्ड वस्तुएं" }, { "Ready", "तैयार" }, { "Success", "सफलता" },
                { "Error", "त्रुटि" }, { "Warning", "चेतावनी" }, { "Information", "सूचना" }, { "Select Language", "भाषा चुनें" },
                { "Import Custom Language", "कस्टम भाषा आयात करें" }, { "Export Template", "टम्प्लेट निर्यात करें" },
                { "Apply Language", "भाषा लागू करें" }
            };
            _translations["hi"] = hiDict;

            // 3. MALAY (ms)
            _availableLanguages["ms"] = new LanguageItem { Code = "ms", Name = "Malay (Bahasa Melayu)", FlagSymbol = "🇲🇾", IsCustom = false };
            var msDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Home", "Utama" }, { "Master", "Induk" }, { "Transaction", "Transaksi" }, { "Accounts", "Akaun" },
                { "Vendor", "Pembekal" }, { "Reports", "Laporan" }, { "Utilities", "Utiliti" }, { "Settings", "Tetapan" },
                { "Manual Balance", "Baki Manual" }, { "Sale Settings", "Tetapan Jualan" }, { "Import/Export", "Import/Eksport" },
                { "Roles", "Peranan" }, { "RolePermission", "Kebenaran Peranan" }, { "Tax Management", "Pengurusan Cukai" },
                { "TaxManagement", "Pengurusan Cukai" }, { "ActivityLog", "Log Aktiviti" }, { "Activity Log", "Log Aktiviti" },
                { "Year Closing", "Penutupan Tahun" }, { "Financial Year Closing", "Penutupan Tahun Kewangan" }, { "App Language", "Bahasa Aplikasi" },
                { "Language Settings", "Tetapan Bahasa" }, { "Save", "Simpan" }, { "Clear", "Padam" }, { "Delete", "Hapus" },
                { "Update", "Kemaskini" }, { "Cancel", "Batal" }, { "Print", "Cetak" }, { "Close", "Tutup" }, { "Search", "Cari" },
                { "Filter", "Penapis" }, { "Refresh", "Muat Semula" }, { "Hold", "Pegang (Hold)" }, { "Last Bill", "Resit Terakhir" },
                { "Export", "Eksport" }, { "Import", "Import" }, { "Bill No", "No. Resit" }, { "Customer Name", "Nama Pelanggan" },
                { "Customer", "Pelanggan" }, { "Item Name", "Nama Barangan" }, { "Barcode", "Kod Bar" }, { "Unit", "Unit" },
                { "Qty", "Kuantiti" }, { "Unit Price", "Harga Seunit" }, { "Total Amount", "Jumlah Besar" }, { "Amount", "Jumlah" },
                { "Net Amount", "Jumlah Bersih" }, { "Cost", "Kos" }, { "Selling Price", "Harga Jualan" }, { "Billed By", "Juruwang" },
                { "Date & Time", "Tarikh & Masa" }, { "Date", "Tarikh" }, { "Payment Mode", "Mod Pembayaran" }, { "Total Sales", "Jumlah Jualan" },
                { "Total Orders", "Jumlah Pesanan" }, { "Average Order", "Purata Pesanan" }, { "Total Profit", "Jumlah Keuntungan" },
                { "Items Sold", "Barangan Dijual" }, { "Hold Item", "Item Dipegang" }, { "Ready", "Sedia" }, { "Success", "Berjaya" },
                { "Error", "Ralat" }, { "Warning", "Amaran" }, { "Information", "Maklumat" }, { "Select Language", "Pilih Bahasa" },
                { "Import Custom Language", "Import Bahasa Tersuai" }, { "Export Templat", "Eksport Templat" },
                { "Apply Language", "Gunakan Bahasa" }
            };
            _translations["ms"] = msDict;

            // 4. SPANISH (es)
            _availableLanguages["es"] = new LanguageItem { Code = "es", Name = "Spanish (Español)", FlagSymbol = "🇪🇸", IsCustom = false };
            var esDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Home", "Inicio" }, { "Master", "Principal" }, { "Transaction", "Transacción" }, { "Accounts", "Cuentas" },
                { "Vendor", "Proveedor" }, { "Reports", "Informes" }, { "Utilities", "Utilidades" }, { "Settings", "Configuración" },
                { "Manual Balance", "Saldo Manual" }, { "Sale Settings", "Ajustes de Venta" }, { "Import/Export", "Importar/Exportar" },
                { "Roles", "Roles" }, { "RolePermission", "Permisos de Rol" }, { "Tax Management", "Gestión de Impuestos" },
                { "TaxManagement", "Gestión de Impuestos" }, { "ActivityLog", "Registro de Actividad" }, { "Activity Log", "Registro de Actividad" },
                { "Year Closing", "Cierre Anual" }, { "Financial Year Closing", "Cierre del Ejercicio" }, { "App Language", "Idioma de la Aplicación" },
                { "Language Settings", "Ajustes de Idioma" }, { "Save", "Guardar" }, { "Clear", "Limpiar" }, { "Delete", "Eliminar" },
                { "Update", "Actualizar" }, { "Cancel", "Cancelar" }, { "Print", "Imprimir" }, { "Close", "Cerrar" }, { "Search", "Buscar" },
                { "Filter", "Filtrar" }, { "Refresh", "Actualizar" }, { "Hold", "Retener (Hold)" }, { "Last Bill", "Última Factura" },
                { "Export", "Exportar" }, { "Import", "Importar" }, { "Bill No", "Nº Factura" }, { "Customer Name", "Nombre del Cliente" },
                { "Customer", "Cliente" }, { "Item Name", "Nombre del Artículo" }, { "Barcode", "Código de Barras" }, { "Unit", "Unidad" },
                { "Qty", "Cant" }, { "Unit Price", "Precio Unitario" }, { "Total Amount", "Importe Total" }, { "Amount", "Importe" },
                { "Net Amount", "Importe Neto" }, { "Cost", "Coste" }, { "Selling Price", "Precio de Venta" }, { "Billed By", "Facturado Por" },
                { "Date & Time", "Fecha y Hora" }, { "Date", "Fecha" }, { "Payment Mode", "Forma de Pago" }, { "Total Sales", "Ventas Totales" },
                { "Total Orders", "Pedidos Totales" }, { "Average Order", "Pedido Medio" }, { "Total Profit", "Beneficio Total" },
                { "Items Sold", "Artículos Vendidos" }, { "Hold Item", "Artículos Retenidos" }, { "Ready", "Listo" }, { "Success", "Éxito" },
                { "Error", "Error" }, { "Warning", "Advertencia" }, { "Information", "Información" }, { "Select Language", "Seleccionar Idioma" },
                { "Import Custom Language", "Importar Idioma Personalizado" }, { "Export Template", "Exportar Plantilla" },
                { "Apply Language", "Aplicar Idioma" }
            };
            _translations["es"] = esDict;

            // 5. ITALIAN (it)
            _availableLanguages["it"] = new LanguageItem { Code = "it", Name = "Italian (Italiano)", FlagSymbol = "🇮🇹", IsCustom = false };
            var itDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Home", "Home" }, { "Master", "Anagrafica" }, { "Transaction", "Transazioni" }, { "Accounts", "Contabilità" },
                { "Vendor", "Fornitore" }, { "Reports", "Report" }, { "Utilities", "Utilità" }, { "Settings", "Impostazioni" },
                { "Manual Balance", "Saldo Manuale" }, { "Sale Settings", "Impostazioni Vendita" }, { "Import/Export", "Importa/Esporta" },
                { "Roles", "Ruoli" }, { "RolePermission", "Permessi Ruolo" }, { "Tax Management", "Gestione Tasse" },
                { "TaxManagement", "Gestione Tasse" }, { "ActivityLog", "Registro Attività" }, { "Activity Log", "Registro Attività" },
                { "Year Closing", "Chiusura Anno" }, { "Financial Year Closing", "Chiusura Anno Fiscale" }, { "App Language", "Lingua App" },
                { "Language Settings", "Impostazioni Lingua" }, { "Save", "Salva" }, { "Clear", "Cancella" }, { "Delete", "Elimina" },
                { "Update", "Aggiorna" }, { "Cancel", "Annulla" }, { "Print", "Stampa" }, { "Close", "Chiudi" }, { "Search", "Cerca" },
                { "Filter", "Filtra" }, { "Refresh", "Aggiorna" }, { "Hold", "In Sospeso" }, { "Last Bill", "Ultimo Scontrino" },
                { "Export", "Esporta" }, { "Import", "Importa" }, { "Bill No", "N. Scontrino" }, { "Customer Name", "Nome Cliente" },
                { "Customer", "Cliente" }, { "Item Name", "Nome Articolo" }, { "Barcode", "Codice a Barre" }, { "Unit", "Unità" },
                { "Qty", "Qtà" }, { "Unit Price", "Prezzo Unitario" }, { "Total Amount", "Importo Totale" }, { "Amount", "Importo" },
                { "Net Amount", "Importo Netto" }, { "Cost", "Costo" }, { "Selling Price", "Prezzo Vendita" }, { "Billed By", "Emesso Da" },
                { "Date & Time", "Data e Ora" }, { "Date", "Data" }, { "Payment Mode", "Modalità Pagamento" }, { "Total Sales", "Vendite Totali" },
                { "Total Orders", "Ordini Totali" }, { "Average Order", "Ordine Medio" }, { "Total Profit", "Profitto Totale" },
                { "Items Sold", "Articoli Venduti" }, { "Hold Item", "Articoli in Sospeso" }, { "Ready", "Pronto" }, { "Success", "Operazione Riuscita" },
                { "Error", "Errore" }, { "Warning", "Avviso" }, { "Information", "Informazioni" }, { "Select Language", "Seleziona Lingua" },
                { "Import Custom Language", "Importa Lingua Personalizzata" }, { "Export Template", "Esporta Modello" },
                { "Apply Language", "Applica Lingua" }
            };
            _translations["it"] = itDict;
        }

        private static void LoadCustomLanguages()
        {
            try
            {
                if (!Directory.Exists(LanguagesDirectory)) return;

                string[] files = Directory.GetFiles(LanguagesDirectory, "*.*")
                    .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                foreach (var file in files)
                {
                    try
                    {
                        LoadLanguageFromFile(file);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load custom language file '{file}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scanning custom language directory: {ex.Message}");
            }
        }

        private static void LoadSavedConfiguration()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath, Encoding.UTF8).Trim();
                    if (!string.IsNullOrEmpty(json))
                    {
                        string code = ExtractValueFromJson(json, "LanguageCode");
                        if (!string.IsNullOrEmpty(code) && _availableLanguages.ContainsKey(code))
                        {
                            _currentLanguageCode = code;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved language configuration: {ex.Message}");
            }
        }

        public static void SaveConfiguration()
        {
            try
            {
                EnsureDirectoriesExist();
                string json = $"{{\n  \"LanguageCode\": \"{_currentLanguageCode}\",\n  \"LanguageName\": \"{CurrentLanguageName}\"\n}}";
                File.WriteAllText(ConfigFilePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving language configuration: {ex.Message}");
            }
        }

        public static bool SetLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode)) return false;

            if (_availableLanguages.ContainsKey(languageCode))
            {
                _currentLanguageCode = languageCode;
                SaveConfiguration();
                LanguageChanged?.Invoke(null, EventArgs.Empty);
                return true;
            }

            return false;
        }

        public static string GetString(string textKey, string fallback = null)
        {
            if (string.IsNullOrWhiteSpace(textKey)) return fallback ?? string.Empty;

            string trimmed = textKey.Trim();
            bool hasColon = trimmed.EndsWith(":") || textKey.TrimEnd().EndsWith(":");
            string coreKey = trimmed.TrimEnd(':').Trim();

            string translation = LookupKey(coreKey);
            if (translation == null)
            {
                translation = LookupKey(trimmed);
            }
            if (translation == null)
            {
                translation = LookupKey(textKey);
            }

            if (translation != null)
            {
                if (hasColon && !translation.EndsWith(":"))
                {
                    translation = translation + " :";
                }
                return translation;
            }

            return fallback ?? textKey;
        }

        private static string LookupKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            if (_translations.TryGetValue(_currentLanguageCode, out var langDict))
            {
                if (langDict.TryGetValue(key, out var translation) && !string.IsNullOrWhiteSpace(translation))
                {
                    return translation;
                }
            }

            // Fallback to English dictionary if present
            if (_translations.TryGetValue("en", out var enDict))
            {
                if (enDict.TryGetValue(key, out var enTranslation) && !string.IsNullOrWhiteSpace(enTranslation))
                {
                    return enTranslation;
                }
            }

            return null;
        }

        private static string GetOrStoreOriginalText(object obj, string currentText)
        {
            if (obj == null || string.IsNullOrWhiteSpace(currentText)) return currentText;

            if (_originalTextMap.TryGetValue(obj, out string original))
            {
                return original;
            }
            else
            {
                _originalTextMap.Add(obj, currentText);
                return currentText;
            }
        }

        public static void ApplyLanguageToForm(Form form)
        {
            if (form == null || form.IsDisposed) return;

            try
            {
                form.SuspendLayout();

                if (!string.IsNullOrWhiteSpace(form.Text))
                {
                    string origText = GetOrStoreOriginalText(form, form.Text);
                    form.Text = GetString(origText, origText);
                }

                ApplyLanguageToControls(form.Controls);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying language to form '{form?.Name}': {ex.Message}");
            }
            finally
            {
                form.ResumeLayout(false);
            }
        }

        public static void ApplyLanguageToControls(Control.ControlCollection controls)
        {
            if (controls == null) return;

            foreach (Control control in controls)
            {
                if (control == null) continue;

                // Skip user data input controls to preserve typed values
                bool isInputControl = control is TextBox || 
                                       control is Infragistics.Win.UltraWinEditors.UltraTextEditor || 
                                       control is ComboBox || 
                                       control is DateTimePicker || 
                                       control is NumericUpDown || 
                                       control is RichTextBox || 
                                       control is MaskedTextBox;

                if (!isInputControl && !string.IsNullOrWhiteSpace(control.Text))
                {
                    string orig = GetOrStoreOriginalText(control, control.Text);
                    string translated = GetString(orig, orig);
                    if (!string.Equals(control.Text, translated, StringComparison.Ordinal))
                    {
                        control.Text = translated;
                    }
                }

                // Special handling for UltraGrid (Infragistics Grid)
                if (control is UltraGrid uGrid)
                {
                    foreach (UltraGridBand band in uGrid.DisplayLayout.Bands)
                    {
                        foreach (UltraGridColumn col in band.Columns)
                        {
                            if (col != null && !string.IsNullOrWhiteSpace(col.Header.Caption))
                            {
                                string orig = GetOrStoreOriginalText(col, col.Header.Caption);
                                col.Header.Caption = GetString(orig, orig);
                            }
                        }
                    }
                }
                // Special handling for DataGridView
                else if (control is DataGridView dgv)
                {
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        if (col != null && !string.IsNullOrWhiteSpace(col.HeaderText))
                        {
                            string orig = GetOrStoreOriginalText(col, col.HeaderText);
                            col.HeaderText = GetString(orig, orig);
                        }
                    }
                }
                // Special handling for ToolStrip / StatusStrip / MenuStrip
                else if (control is ToolStrip toolStrip)
                {
                    foreach (ToolStripItem item in toolStrip.Items)
                    {
                        ApplyLanguageToToolStripItem(item);
                    }
                }
                // Special handling for UltraTabControl (Infragistics Tab Control)
                else if (control is Infragistics.Win.UltraWinTabControl.UltraTabControl uTabControl)
                {
                    foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in uTabControl.Tabs)
                    {
                        if (tab != null && !string.IsNullOrWhiteSpace(tab.Text))
                        {
                            string orig = GetOrStoreOriginalText(tab, tab.Text);
                            tab.Text = GetString(orig, orig);
                        }
                    }
                }
                // Special handling for standard TabControl
                else if (control is TabControl stdTabControl)
                {
                    foreach (TabPage page in stdTabControl.TabPages)
                    {
                        if (page != null && !string.IsNullOrWhiteSpace(page.Text))
                        {
                            string orig = GetOrStoreOriginalText(page, page.Text);
                            page.Text = GetString(orig, orig);
                        }
                    }
                }

                if (control.HasChildren)
                {
                    ApplyLanguageToControls(control.Controls);
                }
            }
        }

        private static void ApplyLanguageToToolStripItem(ToolStripItem item)
        {
            if (item == null) return;

            if (!string.IsNullOrWhiteSpace(item.Text))
            {
                string orig = GetOrStoreOriginalText(item, item.Text);
                item.Text = GetString(orig, orig);
            }

            if (item is ToolStripMenuItem menuItem && menuItem.HasDropDownItems)
            {
                foreach (ToolStripItem subItem in menuItem.DropDownItems)
                {
                    ApplyLanguageToToolStripItem(subItem);
                }
            }
        }

        public static void ApplyLanguageToRibbon(UltraToolbarsManager manager)
        {
            if (manager == null) return;

            try
            {
                // Translate shared tool captions
                foreach (ToolBase tool in manager.Tools)
                {
                    if (tool != null && !string.IsNullOrWhiteSpace(tool.SharedProps.Caption))
                    {
                        string orig = GetOrStoreOriginalText(tool, tool.SharedProps.Caption);
                        tool.SharedProps.Caption = GetString(orig, orig);
                    }
                }

                // Translate ribbon tabs, groups, and tool instances
                if (manager.Ribbon != null)
                {
                    foreach (RibbonTab tab in manager.Ribbon.Tabs)
                    {
                        if (tab != null && !string.IsNullOrWhiteSpace(tab.Caption))
                        {
                            string orig = GetOrStoreOriginalText(tab, tab.Caption);
                            tab.Caption = GetString(orig, orig);
                        }

                        foreach (RibbonGroup group in tab.Groups)
                        {
                            if (group != null && !string.IsNullOrWhiteSpace(group.Caption))
                            {
                                string orig = GetOrStoreOriginalText(group, group.Caption);
                                group.Caption = GetString(orig, orig);
                            }

                            foreach (ToolBase tool in group.Tools)
                            {
                                if (tool != null && tool.SharedProps != null && !string.IsNullOrWhiteSpace(tool.SharedProps.Caption))
                                {
                                    string orig = GetOrStoreOriginalText(tool.SharedProps, tool.SharedProps.Caption);
                                    tool.SharedProps.Caption = GetString(orig, orig);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying language to ribbon: {ex.Message}");
            }
        }

        public static bool ImportLanguageFile(string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
                throw new FileNotFoundException("Specified language file does not exist.", sourceFilePath);

            EnsureDirectoriesExist();
            string fileName = Path.GetFileName(sourceFilePath);
            string destPath = Path.Combine(LanguagesDirectory, fileName);

            File.Copy(sourceFilePath, destPath, true);

            string code = LoadLanguageFromFile(destPath);
            if (!string.IsNullOrEmpty(code))
            {
                LanguageChanged?.Invoke(null, EventArgs.Empty);
                return true;
            }

            return false;
        }

        private static string LoadLanguageFromFile(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            string code = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();
            string name = Path.GetFileNameWithoutExtension(filePath);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (ext == ".json")
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                string jsonCode = ExtractValueFromJson(json, "LanguageCode");
                string jsonName = ExtractValueFromJson(json, "LanguageName");

                if (!string.IsNullOrEmpty(jsonCode)) code = jsonCode;
                if (!string.IsNullOrEmpty(jsonName)) name = jsonName;

                dict = ParseSimpleJsonDictionary(json);
            }
            else if (ext == ".csv")
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    string[] parts = line.Split(new[] { ',', ';' }, 2);
                    if (parts.Length == 2)
                    {
                        string k = parts[0].Trim('"', ' ', '\t');
                        string v = parts[1].Trim('"', ' ', '\t');
                        if (!string.IsNullOrEmpty(k)) dict[k] = v;
                    }
                }
            }

            if (dict.Count > 0)
            {
                _availableLanguages[code] = new LanguageItem
                {
                    Code = code,
                    Name = name,
                    FlagSymbol = "🌐",
                    IsCustom = true,
                    FilePath = filePath
                };
                _translations[code] = dict;
                return code;
            }

            return null;
        }

        public static void ExportLanguageTemplate(string targetFilePath)
        {
            if (string.IsNullOrWhiteSpace(targetFilePath)) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"LanguageCode\": \"custom\",");
            sb.AppendLine("  \"LanguageName\": \"My Custom Language\",");
            sb.AppendLine("  \"Translations\": {");

            var enDict = _translations["en"];
            int count = enDict.Count;
            int i = 0;
            foreach (var kvp in enDict)
            {
                i++;
                string comma = (i == count) ? "" : ",";
                sb.AppendLine($"    \"{EscapeJsonString(kvp.Key)}\": \"{EscapeJsonString(kvp.Value)}\"{comma}");
            }

            sb.AppendLine("  }");
            sb.AppendLine("}");

            File.WriteAllText(targetFilePath, sb.ToString(), Encoding.UTF8);
        }

        private static string ExtractValueFromJson(string json, string key)
        {
            try
            {
                string pattern = $"\"{key}\"\\s*:\\s*\"([^\"]*)\"";
                var match = System.Text.RegularExpressions.Regex.Match(json, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            catch { }
            return null;
        }

        private static Dictionary<string, string> ParseSimpleJsonDictionary(string json)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string pattern = $"\"([^\"]+)\"\\s*:\\s*\"([^\"]*)\"";
                var matches = System.Text.RegularExpressions.Regex.Matches(json, pattern);
                foreach (System.Text.RegularExpressions.Match m in matches)
                {
                    string k = m.Groups[1].Value;
                    string v = m.Groups[2].Value;
                    if (k != "LanguageCode" && k != "LanguageName")
                    {
                        dict[k] = v;
                    }
                }
            }
            catch { }
            return dict;
        }

        private static string EscapeJsonString(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }
}
