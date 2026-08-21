using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinExplorerBar;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinTabControl;
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

        // Store original text for every UI element so switching languages multiple times never degrades or locks text in foreign strings
        private static readonly Dictionary<object, string> _originalTextMap = new Dictionary<object, string>();

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

        public static void EnsureDirectoriesExist()
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

        private static string GetOriginalText(object element, string currentText)
        {
            if (element == null) return currentText;
            lock (_originalTextMap)
            {
                if (_originalTextMap.TryGetValue(element, out string orig) && !string.IsNullOrEmpty(orig))
                {
                    return orig;
                }
                if (!string.IsNullOrEmpty(currentText))
                {
                    _originalTextMap[element] = currentText;
                }
            }
            return currentText;
        }

        private static void InitializeBuiltInLanguages()
        {
            // 1. ENGLISH (en)
            _availableLanguages["en"] = new LanguageItem { Code = "en", Name = "English", FlagSymbol = "🇬🇧", IsCustom = false };
            var enDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Home", "Home" }, { "Master", "Master" }, { "Transaction", "Transaction" }, { "Accounts", "Accounts" },
                { "Vendor", "Vendor" }, { "Reports", "Reports" }, { "Utilities", "Utilities" }, { "Settings", "Settings" },
                { "Manual Balance", "Manual Balance" }, { "Sale Settings", "Sale Settings" }, { "Import/Export", "Import/Export" },
                { "Roles", "Roles" }, { "RolePermission", "Role Permission" }, { "Role Permissions", "Role Permissions" },
                { "Tax Management", "Tax Management" }, { "TaxManagement", "Tax Management" }, { "ActivityLog", "Activity Log" },
                { "Activity Log", "Activity Log" }, { "Year Closing", "Year Closing" }, { "Financial Year Closing", "Financial Year Closing" },
                { "App Language", "App Language" }, { "App Language Settings", "App Language Settings" }, { "Language Settings", "Language Settings" },
                { "Select your preferred application language or import custom translations", "Select your preferred application language or import custom translations" },
                { "Available Languages:", "Available Languages:" }, { "Apply Language", "Apply Language" }, { "Reset to Default (English)", "Reset to Default (English)" },
                { "Import Custom Language", "Import Custom Language" }, { "Export Template", "Export Template" }, { "Close", "Close" },
                { "Save", "Save" }, { "Clear", "Clear" }, { "Delete", "Delete" }, { "Update", "Update" }, { "Cancel", "Cancel" }, { "Print", "Print" },
                { "Search", "Search" }, { "Filter", "Filter" }, { "Refresh", "Refresh" }, { "Hold", "Hold" }, { "Last Bill", "Last Bill" },
                { "Export", "Export" }, { "Import", "Import" }, { "Bill No", "Bill No" }, { "Invoice No", "Invoice No" },
                { "Customer Name", "Customer Name" }, { "Customer", "Customer" }, { "Item Name", "Item Name" }, { "Item Description", "Item Description" },
                { "Barcode", "Barcode" }, { "Unit", "Unit" }, { "Qty", "Qty" }, { "Stock Qty", "Stock Qty" }, { "Unit Price", "Unit Price" },
                { "Total Amount", "Total Amount" }, { "Amount", "Amount" }, { "Net Amount", "Net Amount" }, { "Cost", "Cost" },
                { "Selling Price", "Selling Price" }, { "Price", "Price" }, { "Billed By", "Billed By" }, { "Date & Time", "Date & Time" },
                { "Date", "Date" }, { "Time", "Time" }, { "Payment Mode", "Payment Mode" }, { "Cash", "Cash" }, { "Card", "Card" },
                { "Total Sales", "Total Sales" }, { "Total Orders", "Total Orders" }, { "Average Order", "Average Order" },
                { "Total Profit", "Total Profit" }, { "Items Sold", "Items Sold" }, { "Hold Item", "Hold Item" }, { "Ready", "Ready" },
                { "Success", "Success" }, { "Error", "Error" }, { "Warning", "Warning" }, { "Information", "Information" },
                { "Item Master", "Item Master" }, { "ItemMaster", "Item Master" }, { "Sales", "Sales Invoice" }, { "Exit", "Exit" },
                { "Report", "Report" }, { "LogOff", "Log Off" }, { "ReOrder", "ReOrder" }, { "Overview", "Overview" },
                { "Business Summary", "Business Summary" }, { "Dashboard", "Dashboard" }, { "My Favourite Menu", "My Favourite Menu" },
                { "Sales Invoice", "Sales Invoice" }, { "Purchase", "Purchase" }, { "Purchase Order", "Purchase Order" },
                { "Sales Return", "Sales Return" }, { "Purchase Return", "Purchase Return" }, { "Stock Adjustment", "Stock Adjustment" },
                { "Stock Transfer", "Stock Transfer" }, { "Stock Report", "Stock Report" }, { "Category", "Category" },
                { "Group", "Group" }, { "Brand", "Brand" }, { "Users", "Users" }, { "Company", "Company" }, { "Branch", "Branch" },
                { "User", "User" }, { "Business Date", "Business Date" }, { "SQL File Size", "SQL File Size" }, { "Nexoris Version", "Nexoris Version" }
            };
            _translations["en"] = enDict;

            // 2. HINDI (hi)
            _availableLanguages["hi"] = new LanguageItem { Code = "hi", Name = "Hindi (हिन्दी)", FlagSymbol = "🇮🇳", IsCustom = false };
            var hiDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Home", "मुख्य पृष्ठ" }, { "Master", "मास्टर" }, { "Transaction", "लेन-देन" }, { "Accounts", "खाते" },
                { "Vendor", "विक्रेता" }, { "Reports", "रिपोर्ट्स" }, { "Utilities", "उपयोगिताएँ" }, { "Settings", "सेटिंग्स" },
                { "Manual Balance", "मैनुअल बैलेंस" }, { "Sale Settings", "बिक्री सेटिंग्स" }, { "Import/Export", "आयात/निर्यात" },
                { "Roles", "भूमिकाएं" }, { "RolePermission", "अनुमति प्रबंधन" }, { "Role Permissions", "भूमिका अनुमतियां" },
                { "Tax Management", "कर प्रबंधन" }, { "TaxManagement", "कर प्रबंधन" }, { "ActivityLog", "गतिविधि लॉग" },
                { "Activity Log", "गतिविधि लॉग" }, { "Year Closing", "वर्ष समाप्ति" }, { "Financial Year Closing", "वित्तीय वर्ष समाप्ति" },
                { "App Language", "ऐप भाषा" }, { "App Language Settings", "ऐप भाषा सेटिंग्स" }, { "Language Settings", "भाषा सेटिंग्स" },
                { "Select your preferred application language or import custom translations", "अपनी पसंदीदा ऐप भाषा चुनें या कस्टम अनुवाद आयात करें" },
                { "Available Languages:", "उपलब्ध भाषाएं:" }, { "Apply Language", "भाषा लागू करें" }, { "Reset to Default (English)", "डिफ़ॉल्ट पर रीसेट करें (अंग्रेज़ी)" },
                { "Import Custom Language", "कस्टम भाषा आयात करें" }, { "Export Template", "टम्प्लेट निर्यात करें" }, { "Close", "बंद करें" },
                { "Save", "सहेजें" }, { "Clear", "साफ करें" }, { "Delete", "हटाएं" }, { "Update", "अद्यतन करें" }, { "Cancel", "रद्द करें" }, { "Print", "प्रिंट" },
                { "Search", "खोजें" }, { "Filter", "फ़िल्टर" }, { "Refresh", "ताज़ा करें" }, { "Hold", "होल्ड करें" }, { "Last Bill", "अंतिम बिल" },
                { "Export", "निर्यात" }, { "Import", "आयात" }, { "Bill No", "बिल संख्या" }, { "Invoice No", "इनवॉइस संख्या" },
                { "Customer Name", "ग्राहक का नाम" }, { "Customer", "ग्राहक" }, { "Item Name", "वस्तु का नाम" }, { "Item Description", "वस्तु विवरण" },
                { "Barcode", "बारकोड" }, { "Unit", "इकाई" }, { "Qty", "मात्रा" }, { "Stock Qty", "स्टॉक मात्रा" }, { "Unit Price", "इकाई मूल्य" },
                { "Total Amount", "कुल राशि" }, { "Amount", "राशि" }, { "Net Amount", "शुद्ध राशि" }, { "Cost", "लागत" },
                { "Selling Price", "बिक्री मूल्य" }, { "Price", "मूल्य" }, { "Billed By", "बिलकर्ता" }, { "Date & Time", "दिनांक एवं समय" },
                { "Date", "दिनांक" }, { "Time", "समय" }, { "Payment Mode", "भुगतान का प्रकार" }, { "Cash", "नकद" }, { "Card", "कार्ड" },
                { "Total Sales", "कुल बिक्री" }, { "Total Orders", "कुल ऑर्डर" }, { "Average Order", "औसत ऑर्डर" },
                { "Total Profit", "कुल लाभ" }, { "Items Sold", "बेची गई वस्तुएं" }, { "Hold Item", "होल्ड वस्तुएं" }, { "Ready", "तैयार" },
                { "Success", "सफलता" }, { "Error", "त्रुटि" }, { "Warning", "चेतावनी" }, { "Information", "सूचना" },
                { "Item Master", "वस्तु मास्टर" }, { "ItemMaster", "वस्तु मास्टर" }, { "Sales", "बिक्री चालान" }, { "Exit", "बाहर निकलें" },
                { "Report", "रिपोर्ट" }, { "LogOff", "लॉग ऑफ" }, { "ReOrder", "पुनः ऑर्डर" }, { "Overview", "अवलोकन" },
                { "Business Summary", "व्यवसाय सारांश" }, { "Dashboard", "डैशबोर्ड" }, { "My Favourite Menu", "मेरा पसंदीदा मेनू" },
                { "Sales Invoice", "बिक्री चालान" }, { "Purchase", "खरीद" }, { "Purchase Order", "खरीद ऑर्डर" },
                { "Sales Return", "बिक्री वापसी" }, { "Purchase Return", "खरीद वापसी" }, { "Stock Adjustment", "स्टॉक समायोजन" },
                { "Stock Transfer", "स्टॉक स्थानांतरण" }, { "Stock Report", "स्टॉक रिपोर्ट" }, { "Category", "श्रेणी" },
                { "Group", "समूह" }, { "Brand", "ब्रांड" }, { "Users", "उपयोगकर्ता" }, { "Company", "कंपनी" }, { "Branch", "शाखा" },
                { "User", "उपयोगकर्ता" }, { "Business Date", "व्यवसाय दिनांक" }, { "SQL File Size", "एसक्यूएल फ़ाइल आकार" }, { "Nexoris Version", "नेक्सोरिस संस्करण" }
            };
            _translations["hi"] = hiDict;

            // 3. MALAY (ms)
            _availableLanguages["ms"] = new LanguageItem { Code = "ms", Name = "Malay (Bahasa Melayu)", FlagSymbol = "🇲🇾", IsCustom = false };
            var msDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Home", "Utama" }, { "Master", "Induk" }, { "Transaction", "Transaksi" }, { "Accounts", "Akaun" },
                { "Vendor", "Pembekal" }, { "Reports", "Laporan" }, { "Utilities", "Utiliti" }, { "Settings", "Tetapan" },
                { "Manual Balance", "Baki Manual" }, { "Sale Settings", "Tetapan Jualan" }, { "Import/Export", "Import/Eksport" },
                { "Roles", "Peranan" }, { "RolePermission", "Kebenaran Peranan" }, { "Role Permissions", "Kebenaran Peranan" },
                { "Tax Management", "Pengurusan Cukai" }, { "TaxManagement", "Pengurusan Cukai" }, { "ActivityLog", "Log Aktiviti" },
                { "Activity Log", "Log Aktiviti" }, { "Year Closing", "Penutupan Tahun" }, { "Financial Year Closing", "Penutupan Tahun Kewangan" },
                { "App Language", "Bahasa Aplikasi" }, { "App Language Settings", "Tetapan Bahasa Aplikasi" }, { "Language Settings", "Tetapan Bahasa" },
                { "Select your preferred application language or import custom translations", "Pilih bahasa aplikasi pilihan anda atau import terjemahan tersuai" },
                { "Available Languages:", "Bahasa Yang Ada:" }, { "Apply Language", "Gunakan Bahasa" }, { "Reset to Default (English)", "Set Semula ke Laluan (Inggeris)" },
                { "Import Custom Language", "Import Bahasa Tersuai" }, { "Export Template", "Eksport Templat" }, { "Close", "Tutup" },
                { "Save", "Simpan" }, { "Clear", "Padam" }, { "Delete", "Hapus" }, { "Update", "Kemaskini" }, { "Cancel", "Batal" }, { "Print", "Cetak" },
                { "Search", "Cari" }, { "Filter", "Penapis" }, { "Refresh", "Muat Semula" }, { "Hold", "Pegang (Hold)" }, { "Last Bill", "Resit Terakhir" },
                { "Export", "Eksport" }, { "Import", "Import" }, { "Bill No", "No. Resit" }, { "Invoice No", "No. Invois" },
                { "Customer Name", "Nama Pelanggan" }, { "Customer", "Pelanggan" }, { "Item Name", "Nama Barangan" }, { "Item Description", "Penerangan Barangan" },
                { "Barcode", "Kod Bar" }, { "Unit", "Unit" }, { "Qty", "Kuantiti" }, { "Stock Qty", "Kuantiti Stok" }, { "Unit Price", "Harga Seunit" },
                { "Total Amount", "Jumlah Besar" }, { "Amount", "Jumlah" }, { "Net Amount", "Jumlah Bersih" }, { "Cost", "Kos" },
                { "Selling Price", "Harga Jualan" }, { "Price", "Harga" }, { "Billed By", "Juruwang" }, { "Date & Time", "Tarikh & Masa" },
                { "Date", "Tarikh" }, { "Time", "Masa" }, { "Payment Mode", "Mod Pembayaran" }, { "Cash", "Tunai" }, { "Card", "Kad" },
                { "Total Sales", "Jumlah Jualan" }, { "Total Orders", "Jumlah Pesanan" }, { "Average Order", "Purata Pesanan" },
                { "Total Profit", "Jumlah Keuntungan" }, { "Items Sold", "Barangan Dijual" }, { "Hold Item", "Item Dipegang" }, { "Ready", "Sedia" },
                { "Success", "Berjaya" }, { "Error", "Ralat" }, { "Warning", "Amaran" }, { "Information", "Maklumat" },
                { "Item Master", "Induk Barangan" }, { "ItemMaster", "Induk Barangan" }, { "Sales", "Invois Jualan" }, { "Exit", "Keluar" },
                { "Report", "Laporan" }, { "LogOff", "Log Keluar" }, { "ReOrder", "Pesanan Semula" }, { "Overview", "Gambaran Keseluruhan" },
                { "Business Summary", "Ringkasan Perniagaan" }, { "Dashboard", "Papan Pemuka" }, { "My Favourite Menu", "Menu Kegemaran Saya" },
                { "Sales Invoice", "Invois Jualan" }, { "Purchase", "Pembelian" }, { "Purchase Order", "Pesanan Pembelian" },
                { "Sales Return", "Pulangan Jualan" }, { "Purchase Return", "Pulangan Pembelian" }, { "Stock Adjustment", "Pelarasan Stok" },
                { "Stock Transfer", "Pemindahan Stok" }, { "Stock Report", "Laporan Stok" }, { "Category", "Kategori" },
                { "Group", "Kumpulan" }, { "Brand", "Jenama" }, { "Users", "Pengguna" }, { "Company", "Syarikat" }, { "Branch", "Cawangan" },
                { "User", "Pengguna" }, { "Business Date", "Tarikh Perniagaan" }, { "SQL File Size", "Saiz Fail SQL" }, { "Nexoris Version", "Versi Nexoris" }
            };
            _translations["ms"] = msDict;

            // 4. SPANISH (es)
            _availableLanguages["es"] = new LanguageItem { Code = "es", Name = "Spanish (Español)", FlagSymbol = "🇪🇸", IsCustom = false };
            var esDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Home", "Inicio" }, { "Master", "Principal" }, { "Transaction", "Transacción" }, { "Accounts", "Cuentas" },
                { "Vendor", "Proveedor" }, { "Reports", "Informes" }, { "Utilities", "Utilidades" }, { "Settings", "Configuración" },
                { "Manual Balance", "Saldo Manual" }, { "Sale Settings", "Ajustes de Venta" }, { "Import/Export", "Importar/Exportar" },
                { "Roles", "Roles" }, { "RolePermission", "Permisos de Rol" }, { "Role Permissions", "Permisos de Rol" },
                { "Tax Management", "Gestión de Impuestos" }, { "TaxManagement", "Gestión de Impuestos" }, { "ActivityLog", "Registro de Actividad" },
                { "Activity Log", "Registro de Actividad" }, { "Year Closing", "Cierre Anual" }, { "Financial Year Closing", "Cierre del Ejercicio" },
                { "App Language", "Idioma de la Aplicación" }, { "App Language Settings", "Ajustes de Idioma de la Aplicación" }, { "Language Settings", "Ajustes de Idioma" },
                { "Select your preferred application language or import custom translations", "Seleccione su idioma preferido de la aplicación o importe traducciones personalizadas" },
                { "Available Languages:", "Idiomas Disponibles:" }, { "Apply Language", "Aplicar Idioma" }, { "Reset to Default (English)", "Restablecer a Predeterminado (Inglés)" },
                { "Import Custom Language", "Importar Idioma Personalizado" }, { "Export Template", "Exportar Plantilla" }, { "Close", "Cerrar" },
                { "Save", "Guardar" }, { "Clear", "Limpiar" }, { "Delete", "Eliminar" }, { "Update", "Actualizar" }, { "Cancel", "Cancelar" }, { "Print", "Imprimir" },
                { "Search", "Buscar" }, { "Filter", "Filtrar" }, { "Refresh", "Actualizar" }, { "Hold", "Retener (Hold)" }, { "Last Bill", "Última Factura" },
                { "Export", "Exportar" }, { "Import", "Importar" }, { "Bill No", "Nº Factura" }, { "Invoice No", "Nº Factura" },
                { "Customer Name", "Nombre del Cliente" }, { "Customer", "Cliente" }, { "Item Name", "Nombre del Artículo" }, { "Item Description", "Descripción del Artículo" },
                { "Barcode", "Código de Barras" }, { "Unit", "Unidad" }, { "Qty", "Cant" }, { "Stock Qty", "Stock Cant" }, { "Unit Price", "Precio Unitario" },
                { "Total Amount", "Importe Total" }, { "Amount", "Importe" }, { "Net Amount", "Importe Neto" }, { "Cost", "Coste" },
                { "Selling Price", "Precio de Venta" }, { "Price", "Precio" }, { "Billed By", "Facturado Por" }, { "Date & Time", "Fecha y Hora" },
                { "Date", "Fecha" }, { "Time", "Hora" }, { "Payment Mode", "Forma de Pago" }, { "Cash", "Efectivo" }, { "Card", "Tarjeta" },
                { "Total Sales", "Ventas Totales" }, { "Total Orders", "Pedidos Totales" }, { "Average Order", "Pedido Medio" },
                { "Total Profit", "Beneficio Total" }, { "Items Sold", "Artículos Vendidos" }, { "Hold Item", "Artículos Retenidos" }, { "Ready", "Listo" },
                { "Success", "Éxito" }, { "Error", "Error" }, { "Warning", "Advertencia" }, { "Information", "Información" },
                { "Item Master", "Maestro de Artículos" }, { "ItemMaster", "Maestro de Artículos" }, { "Sales", "Factura de Venta" }, { "Exit", "Salir" },
                { "Report", "Informe" }, { "LogOff", "Cerrar Sesión" }, { "ReOrder", "Reordenar" }, { "Overview", "Resumen" },
                { "Business Summary", "Resumen de Negocio" }, { "Dashboard", "Panel de Control" }, { "My Favourite Menu", "Mi Menú Favorito" },
                { "Sales Invoice", "Factura de Venta" }, { "Purchase", "Compra" }, { "Purchase Order", "Orden de Compra" },
                { "Sales Return", "Devolución de Venta" }, { "Purchase Return", "Devolución de Compra" }, { "Stock Adjustment", "Ajuste de Inventario" },
                { "Stock Transfer", "Transferencia de Stock" }, { "Stock Report", "Informe de Stock" }, { "Category", "Categoría" },
                { "Group", "Grupo" }, { "Brand", "Marca" }, { "Users", "Usuarios" }, { "Company", "Empresa" }, { "Branch", "Sucursal" },
                { "User", "Usuario" }, { "Business Date", "Fecha Comercial" }, { "SQL File Size", "Tamaño de Archivo SQL" }, { "Nexoris Version", "Versión de Nexoris" }
            };
            _translations["es"] = esDict;

            // 5. ITALIAN (it)
            _availableLanguages["it"] = new LanguageItem { Code = "it", Name = "Italian (Italiano)", FlagSymbol = "🇮🇹", IsCustom = false };
            var itDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Home", "Home" }, { "Master", "Anagrafica" }, { "Transaction", "Transazioni" }, { "Accounts", "Contabilità" },
                { "Vendor", "Fornitore" }, { "Reports", "Report" }, { "Utilities", "Utilità" }, { "Settings", "Impostazioni" },
                { "Manual Balance", "Saldo Manuale" }, { "Sale Settings", "Impostazioni Vendita" }, { "Import/Export", "Importa/Esporta" },
                { "Roles", "Ruoli" }, { "RolePermission", "Permessi Ruolo" }, { "Role Permissions", "Permessi Ruolo" },
                { "Tax Management", "Gestione Tasse" }, { "TaxManagement", "Gestione Tasse" }, { "ActivityLog", "Registro Attività" },
                { "Activity Log", "Registro Attività" }, { "Year Closing", "Chiusura Anno" }, { "Financial Year Closing", "Chiusura Anno Fiscale" },
                { "App Language", "Lingua App" }, { "App Language Settings", "Impostazioni Lingua App" }, { "Language Settings", "Impostazioni Lingua" },
                { "Select your preferred application language or import custom translations", "Seleziona la tua lingua preferita dell'applicazione o importa traduzioni personalizzate" },
                { "Available Languages:", "Lingue Disponibili:" }, { "Apply Language", "Applica Lingua" }, { "Reset to Default (English)", "Ripristina Predefinito (Inglese)" },
                { "Import Custom Language", "Importa Lingua Personalizzata" }, { "Export Template", "Esporta Modello" }, { "Close", "Chiudi" },
                { "Save", "Salva" }, { "Clear", "Cancella" }, { "Delete", "Elimina" }, { "Update", "Aggiorna" }, { "Cancel", "Annulla" }, { "Print", "Stampa" },
                { "Search", "Cerca" }, { "Filter", "Filtra" }, { "Refresh", "Aggiorna" }, { "Hold", "In Sospeso" }, { "Last Bill", "Ultimo Scontrino" },
                { "Export", "Esporta" }, { "Import", "Importa" }, { "Bill No", "N. Scontrino" }, { "Invoice No", "N. Fattura" },
                { "Customer Name", "Nome Cliente" }, { "Customer", "Cliente" }, { "Item Name", "Nome Articolo" }, { "Item Description", "Descrizione Articolo" },
                { "Barcode", "Codice a Barre" }, { "Unit", "Unità" }, { "Qty", "Qtà" }, { "Stock Qty", "Qtà Giacenza" }, { "Unit Price", "Prezzo Unitario" },
                { "Total Amount", "Importo Totale" }, { "Amount", "Importo" }, { "Net Amount", "Importo Netto" }, { "Cost", "Costo" },
                { "Selling Price", "Prezzo Vendita" }, { "Price", "Prezzo" }, { "Billed By", "Emesso Da" }, { "Date & Time", "Data e Ora" },
                { "Date", "Data" }, { "Time", "Ora" }, { "Payment Mode", "Modalità Pagamento" }, { "Cash", "Contanti" }, { "Card", "Carta" },
                { "Total Sales", "Vendite Totali" }, { "Total Orders", "Ordini Totali" }, { "Average Order", "Ordine Medio" },
                { "Total Profit", "Profitto Totale" }, { "Items Sold", "Articoli Venduti" }, { "Hold Item", "Articoli in Sospeso" }, { "Ready", "Pronto" },
                { "Success", "Operazione Riuscita" }, { "Error", "Errore" }, { "Warning", "Avviso" }, { "Information", "Informazioni" },
                { "Item Master", "Anagrafica Articoli" }, { "ItemMaster", "Anagrafica Articoli" }, { "Sales", "Fattura di Vendita" }, { "Exit", "Esci" },
                { "Report", "Report" }, { "LogOff", "Disconnetti" }, { "ReOrder", "Riapprovvigionamento" }, { "Overview", "Panoramica" },
                { "Business Summary", "Riepilogo Aziendale" }, { "Dashboard", "Cruscotto" }, { "My Favourite Menu", "Il Mio Menu Preferito" },
                { "Sales Invoice", "Fattura di Vendita" }, { "Purchase", "Acquisto" }, { "Purchase Order", "Ordine di Acquisto" },
                { "Sales Return", "Reso Vendita" }, { "Purchase Return", "Reso Acquisto" }, { "Stock Adjustment", "Rettifica Inventario" },
                { "Stock Transfer", "Trasferimento Stock" }, { "Stock Report", "Report Giacenze" }, { "Category", "Categoria" },
                { "Group", "Gruppo" }, { "Brand", "Marca" }, { "Users", "Utenti" }, { "Company", "Azienda" }, { "Branch", "Filiale" },
                { "User", "Utente" }, { "Business Date", "Data Aziendale" }, { "SQL File Size", "Dimensione File SQL" }, { "Nexoris Version", "Versione Nexoris" }
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

            string key = textKey.Trim();

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

            return fallback ?? textKey;
        }

        public static void ApplyLanguageToForm(Form form)
        {
            if (form == null || form.IsDisposed) return;

            try
            {
                form.SuspendLayout();

                if (!string.IsNullOrWhiteSpace(form.Text))
                {
                    string origTitle = GetOriginalText(form, form.Text);
                    form.Text = GetString(origTitle, form.Text);
                }

                ApplyLanguageToControls(form.Controls);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying language to form '{form?.Name}': {ex.Message}");
            }
            finally
            {
                try { form.ResumeLayout(false); } catch { }
            }
        }

        public static void ApplyLanguageToControls(Control.ControlCollection controls)
        {
            if (controls == null) return;

            foreach (Control control in controls)
            {
                if (control == null || control.IsDisposed) continue;

                try
                {
                    // Standard Windows Forms & Infragistics Controls
                    if (control is Label lbl && !string.IsNullOrWhiteSpace(lbl.Text))
                    {
                        string orig = GetOriginalText(lbl, lbl.Text);
                        lbl.Text = GetString(orig, lbl.Text);
                    }
                    else if (control is UltraLabel ulbl && !string.IsNullOrWhiteSpace(ulbl.Text))
                    {
                        string orig = GetOriginalText(ulbl, ulbl.Text);
                        ulbl.Text = GetString(orig, ulbl.Text);
                    }
                    else if (control.GetType().Name.Equals("UltraFormattedTextLabel", StringComparison.OrdinalIgnoreCase))
                    {
                        var valProp = control.GetType().GetProperty("Value");
                        if (valProp != null)
                        {
                            string txt = valProp.GetValue(control)?.ToString();
                            if (!string.IsNullOrWhiteSpace(txt))
                            {
                                string orig = GetOriginalText(control, txt);
                                valProp.SetValue(control, GetString(orig, txt));
                            }
                        }
                    }
                    else if (control is Button btn && !string.IsNullOrWhiteSpace(btn.Text))
                    {
                        string orig = GetOriginalText(btn, btn.Text);
                        btn.Text = GetString(orig, btn.Text);
                    }
                    else if (control is UltraButton ubtn && !string.IsNullOrWhiteSpace(ubtn.Text))
                    {
                        string orig = GetOriginalText(ubtn, ubtn.Text);
                        ubtn.Text = GetString(orig, ubtn.Text);
                    }
                    else if (control is CheckBox cb && !string.IsNullOrWhiteSpace(cb.Text))
                    {
                        string orig = GetOriginalText(cb, cb.Text);
                        cb.Text = GetString(orig, cb.Text);
                    }
                    else if (control is UltraCheckEditor ucb && !string.IsNullOrWhiteSpace(ucb.Text))
                    {
                        string orig = GetOriginalText(ucb, ucb.Text);
                        ucb.Text = GetString(orig, ucb.Text);
                    }
                    else if (control is RadioButton rb && !string.IsNullOrWhiteSpace(rb.Text))
                    {
                        string orig = GetOriginalText(rb, rb.Text);
                        rb.Text = GetString(orig, rb.Text);
                    }
                    else if (control is GroupBox gb && !string.IsNullOrWhiteSpace(gb.Text))
                    {
                        string orig = GetOriginalText(gb, gb.Text);
                        gb.Text = GetString(orig, gb.Text);
                    }
                    else if (control is UltraGroupBox ugb && !string.IsNullOrWhiteSpace(ugb.Text))
                    {
                        string orig = GetOriginalText(ugb, ugb.Text);
                        ugb.Text = GetString(orig, ugb.Text);
                    }
                    else if (control is UltraExpandableGroupBox uegb && !string.IsNullOrWhiteSpace(uegb.Text))
                    {
                        string orig = GetOriginalText(uegb, uegb.Text);
                        uegb.Text = GetString(orig, uegb.Text);
                    }
                    else if (control is UltraOptionSet uopt)
                    {
                        foreach (ValueListItem item in uopt.Items)
                        {
                            if (item != null && !string.IsNullOrWhiteSpace(item.DisplayText))
                            {
                                string orig = GetOriginalText(item, item.DisplayText);
                                item.DisplayText = GetString(orig, item.DisplayText);
                            }
                        }
                    }
                    else if (control is TabControl tc)
                    {
                        foreach (TabPage tp in tc.TabPages)
                        {
                            if (tp != null && !string.IsNullOrWhiteSpace(tp.Text))
                            {
                                string orig = GetOriginalText(tp, tp.Text);
                                tp.Text = GetString(orig, tp.Text);
                            }
                        }
                    }
                    else if (control is UltraTabControl utc)
                    {
                        foreach (UltraTab tab in utc.Tabs)
                        {
                            if (tab != null && !string.IsNullOrWhiteSpace(tab.Text))
                            {
                                string orig = GetOriginalText(tab, tab.Text);
                                tab.Text = GetString(orig, tab.Text);
                            }
                        }
                    }
                    else if (control is UltraExplorerBar ueb)
                    {
                        ApplyLanguageToExplorerBar(ueb);
                    }
                    else if (control.GetType().Name.Equals("UltraStatusBar", StringComparison.OrdinalIgnoreCase))
                    {
                        ApplyLanguageToStatusBar(control);
                    }
                    else if (control is DataGridView dgv)
                    {
                        foreach (DataGridViewColumn col in dgv.Columns)
                        {
                            if (col != null && !string.IsNullOrWhiteSpace(col.HeaderText))
                            {
                                string orig = GetOriginalText(col, col.HeaderText);
                                col.HeaderText = GetString(orig, col.HeaderText);
                            }
                        }
                    }
                    else if (control is UltraGrid uGrid)
                    {
                        if (uGrid.DisplayLayout != null)
                        {
                            if (uGrid.DisplayLayout.GroupByBox != null && !string.IsNullOrWhiteSpace(uGrid.DisplayLayout.GroupByBox.Prompt))
                            {
                                string origPrompt = GetOriginalText(uGrid.DisplayLayout.GroupByBox, uGrid.DisplayLayout.GroupByBox.Prompt);
                                uGrid.DisplayLayout.GroupByBox.Prompt = GetString(origPrompt, uGrid.DisplayLayout.GroupByBox.Prompt);
                            }

                            foreach (UltraGridBand band in uGrid.DisplayLayout.Bands)
                            {
                                foreach (UltraGridColumn col in band.Columns)
                                {
                                    if (col != null && !string.IsNullOrWhiteSpace(col.Header.Caption))
                                    {
                                        string orig = GetOriginalText(col, col.Header.Caption);
                                        col.Header.Caption = GetString(orig, col.Header.Caption);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing control '{control?.Name}': {ex.Message}");
                }

                if (control.HasChildren)
                {
                    ApplyLanguageToControls(control.Controls);
                }
            }
        }

        public static void ApplyLanguageToExplorerBar(UltraExplorerBar explorerBar)
        {
            if (explorerBar == null) return;

            try
            {
                foreach (UltraExplorerBarGroup group in explorerBar.Groups)
                {
                    if (group != null && !string.IsNullOrWhiteSpace(group.Text))
                    {
                        string orig = GetOriginalText(group, group.Text);
                        group.Text = GetString(orig, group.Text);
                    }

                    foreach (UltraExplorerBarItem item in group.Items)
                    {
                        if (item != null && !string.IsNullOrWhiteSpace(item.Text))
                        {
                            string orig = GetOriginalText(item, item.Text);
                            item.Text = GetString(orig, item.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying language to UltraExplorerBar: {ex.Message}");
            }
        }

        public static void ApplyLanguageToStatusBar(object statusBar)
        {
            if (statusBar == null) return;

            try
            {
                var panelsProp = statusBar.GetType().GetProperty("Panels");
                if (panelsProp != null)
                {
                    var panels = panelsProp.GetValue(statusBar) as System.Collections.IEnumerable;
                    if (panels != null)
                    {
                        foreach (object panel in panels)
                        {
                            if (panel != null)
                            {
                                var textProp = panel.GetType().GetProperty("Text");
                                if (textProp != null)
                                {
                                    string currentText = textProp.GetValue(panel) as string;
                                    if (!string.IsNullOrWhiteSpace(currentText))
                                    {
                                        string orig = GetOriginalText(panel, currentText);
                                        textProp.SetValue(panel, GetString(orig, currentText));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying language to UltraStatusBar: {ex.Message}");
            }
        }

        public static void ApplyLanguageToRibbon(UltraToolbarsManager manager)
        {
            if (manager == null) return;

            try
            {
                // 1. Shared Tools in manager.Tools
                foreach (ToolBase tool in manager.Tools)
                {
                    if (tool != null && tool.SharedProps != null && !string.IsNullOrWhiteSpace(tool.SharedProps.Caption))
                    {
                        string orig = GetOriginalText(tool.SharedProps, tool.SharedProps.Caption);
                        tool.SharedProps.Caption = GetString(orig, tool.SharedProps.Caption);
                    }
                }

                // 2. Ribbon Tabs, Groups, and Group Tools
                if (manager.Ribbon != null)
                {
                    foreach (RibbonTab tab in manager.Ribbon.Tabs)
                    {
                        if (tab != null && !string.IsNullOrWhiteSpace(tab.Caption))
                        {
                            string orig = GetOriginalText(tab, tab.Caption);
                            tab.Caption = GetString(orig, tab.Caption);
                        }

                        foreach (RibbonGroup group in tab.Groups)
                        {
                            if (group != null && !string.IsNullOrWhiteSpace(group.Caption))
                            {
                                string orig = GetOriginalText(group, group.Caption);
                                group.Caption = GetString(orig, group.Caption);
                            }

                            if (group != null && group.Tools != null)
                            {
                                foreach (ToolBase groupTool in group.Tools)
                                {
                                    if (groupTool != null)
                                    {
                                        if (groupTool.SharedProps != null && !string.IsNullOrWhiteSpace(groupTool.SharedProps.Caption))
                                        {
                                            string orig = GetOriginalText(groupTool.SharedProps, groupTool.SharedProps.Caption);
                                            groupTool.SharedProps.Caption = GetString(orig, groupTool.SharedProps.Caption);
                                        }
                                        if (groupTool.InstanceProps != null && !string.IsNullOrWhiteSpace(groupTool.InstanceProps.Caption))
                                        {
                                            string orig = GetOriginalText(groupTool.InstanceProps, groupTool.InstanceProps.Caption);
                                            groupTool.InstanceProps.Caption = GetString(orig, groupTool.InstanceProps.Caption);
                                        }
                                    }
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

        public static void ApplyLanguageToApplication()
        {
            try
            {
                List<Form> openForms = Application.OpenForms.Cast<Form>().Where(f => f != null && !f.IsDisposed).ToList();
                foreach (Form form in openForms)
                {
                    if (form is Home homeForm)
                    {
                        homeForm.ApplyLanguageToAllForms();
                    }
                    else
                    {
                        ApplyLanguageToForm(form);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying language to application: {ex.Message}");
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
