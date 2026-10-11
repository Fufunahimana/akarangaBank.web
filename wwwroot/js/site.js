(() => {
    const translations = {
        fr: {
            "nav.workspace": "Espace bancaire", "nav.overview": "VUE D’ENSEMBLE", "nav.dashboard": "Tableau de bord",
            "nav.customers": "CLIENTS ET AGENCES", "nav.clients": "Clients", "nav.agencies": "Agences", "nav.accounts": "Comptes",
            "nav.operations": "OPÉRATIONS", "nav.journal": "Journal", "nav.deposit": "Dépôt", "nav.withdrawal": "Retrait",
            "nav.transfer": "Virement", "nav.interbranch": "Virements interagences", "nav.loans": "Prêts", "nav.cards": "Cartes",
            "nav.administration": "ADMINISTRATION", "nav.amlApprovals": "Approbations AML", "nav.users": "Utilisateurs",
            "nav.aml": "Alertes AML", "nav.compensations": "Compensations", "nav.cnrf": "CNRF", "nav.amlThresholds": "Seuils AML",
            "nav.settings": "Paramètres", "nav.systemOperational": "Système opérationnel", "nav.myAccount": "Mon compte",
            "nav.logout": "Déconnexion", "nav.login": "Connexion",
            "dashboard.eyebrow": "VUE D’ENSEMBLE", "dashboard.title": "Tableau de bord",
            "dashboard.subtitle": "Suivez l’activité et les indicateurs de votre banque.", "dashboard.approvals": "Voir les virements à approuver",
            "dashboard.clients": "Total des clients", "dashboard.registeredClients": "Clients enregistrés",
            "dashboard.agencies": "Total des agences", "dashboard.network": "Agences dans le réseau",
            "dashboard.deposits": "Dépôts des clients", "dashboard.customerBalances": "Soldes des comptes clients, par devise",
            "dashboard.toReview": "À TRAITER", "dashboard.pendingTitle": "Virements interagences",
            "dashboard.pendingDescription": "Chaque transfert reste en attente jusqu’à sa validation par un administrateur.",
            "dashboard.noPending": "Aucun virement à approuver", "dashboard.allCaughtUp": "La file de validation est à jour.",
            "dashboard.pending": "En attente", "dashboard.seeAll": "Ouvrir la file de validation",
            "dashboard.shortcuts": "RACCOURCIS", "dashboard.quickActions": "Actions rapides",
            "dashboard.manageClients": "Gérer les clients", "dashboard.manageAgencies": "Gérer les agences",
            "dashboard.manageUsers": "Gérer les utilisateurs", "dashboard.approvalRule": "Contrôle des transferts",
            "dashboard.approvalRuleDescription": "Aucun virement interagence n’est comptabilisé avant l’approbation d’un administrateur.",
            "settings.eyebrow": "PRÉFÉRENCES DE L’APPLICATION", "settings.title": "Paramètres",
            "settings.subtitle": "Personnalisez l’apparence et la langue de votre espace de travail.",
            "settings.appearance": "Apparence", "settings.appearanceDescription": "Choisissez le thème affiché dans l’application.",
            "settings.darkMode": "Mode nuit", "settings.darkModeDescription": "Réduit la luminosité de l’interface.",
            "settings.language": "Langue", "settings.languageDescription": "Sélectionnez la langue de l’interface.",
            "settings.interfaceLanguage": "Langue de l’application", "settings.security": "Sécurité et opérations",
            "settings.securityDescription": "Règles actives pour les opérations bancaires.",
            "settings.interbranchTransfers": "Virements interagences",
            "settings.interbranchDescription": "Validation administrateur obligatoire avant comptabilisation.",
            "settings.mandatory": "Obligatoire", "settings.currency": "Devise principale",
            "settings.currencyDescription": "Les montants sont présentés dans la devise de chaque compte.",
            "settings.session": "Session", "settings.sessionDescription": "Expiration après 60 minutes d’inactivité.",
            "settings.savedLocally": "Vos préférences d’affichage sont enregistrées sur cet appareil.",
            "title.Home.Index": "Tableau de bord", "title.Home.Settings": "Paramètres"
        },
        en: {
            "nav.workspace": "Bank workspace", "nav.overview": "OVERVIEW", "nav.dashboard": "Dashboard",
            "nav.customers": "CUSTOMERS AND BRANCHES", "nav.clients": "Customers", "nav.agencies": "Branches", "nav.accounts": "Accounts",
            "nav.operations": "OPERATIONS", "nav.journal": "Journal", "nav.deposit": "Deposit", "nav.withdrawal": "Withdrawal",
            "nav.transfer": "Transfer", "nav.interbranch": "Interbranch transfers", "nav.loans": "Loans", "nav.cards": "Cards",
            "nav.administration": "ADMINISTRATION", "nav.amlApprovals": "AML approvals", "nav.users": "Users",
            "nav.aml": "AML alerts", "nav.compensations": "Settlements", "nav.cnrf": "CNRF", "nav.amlThresholds": "AML thresholds",
            "nav.settings": "Settings", "nav.systemOperational": "System operational", "nav.myAccount": "My account",
            "nav.logout": "Sign out", "nav.login": "Sign in",
            "dashboard.eyebrow": "OVERVIEW", "dashboard.title": "Dashboard",
            "dashboard.subtitle": "Monitor your bank’s activity and key figures.", "dashboard.approvals": "Review transfers awaiting approval",
            "dashboard.clients": "Total customers", "dashboard.registeredClients": "Registered customers",
            "dashboard.agencies": "Total branches", "dashboard.network": "Branches in the network",
            "dashboard.deposits": "Customer deposits", "dashboard.customerBalances": "Customer account balances by currency",
            "dashboard.toReview": "NEEDS REVIEW", "dashboard.pendingTitle": "Interbranch transfers",
            "dashboard.pendingDescription": "Each transfer stays pending until an administrator approves it.",
            "dashboard.noPending": "No transfers to approve", "dashboard.allCaughtUp": "The approval queue is up to date.",
            "dashboard.pending": "Pending", "dashboard.seeAll": "Open the approval queue",
            "dashboard.shortcuts": "SHORTCUTS", "dashboard.quickActions": "Quick actions",
            "dashboard.manageClients": "Manage customers", "dashboard.manageAgencies": "Manage branches",
            "dashboard.manageUsers": "Manage users", "dashboard.approvalRule": "Transfer controls",
            "dashboard.approvalRuleDescription": "Interbranch transfers are not posted until an administrator approves them.",
            "settings.eyebrow": "APPLICATION PREFERENCES", "settings.title": "Settings",
            "settings.subtitle": "Customize the appearance and language of your workspace.",
            "settings.appearance": "Appearance", "settings.appearanceDescription": "Choose the application theme.",
            "settings.darkMode": "Dark mode", "settings.darkModeDescription": "Reduces the brightness of the interface.",
            "settings.language": "Language", "settings.languageDescription": "Choose the interface language.",
            "settings.interfaceLanguage": "Application language", "settings.security": "Security and operations",
            "settings.securityDescription": "Active rules for banking operations.",
            "settings.interbranchTransfers": "Interbranch transfers",
            "settings.interbranchDescription": "Administrator approval is required before posting.",
            "settings.mandatory": "Required", "settings.currency": "Primary currency",
            "settings.currencyDescription": "Amounts are shown in each account’s currency.",
            "settings.session": "Session", "settings.sessionDescription": "Expires after 60 minutes of inactivity.",
            "settings.savedLocally": "Your display preferences are saved on this device.",
            "title.Home.Index": "Dashboard", "title.Home.Settings": "Settings"
        },
        sw: {
            "nav.workspace": "Eneo la benki", "nav.overview": "MUHTASARI", "nav.dashboard": "Dashibodi",
            "nav.customers": "WATEJA NA MATAWI", "nav.clients": "Wateja", "nav.agencies": "Matawi", "nav.accounts": "Akaunti",
            "nav.operations": "SHUGHULI", "nav.journal": "Rejesta", "nav.deposit": "Amana", "nav.withdrawal": "Utoaji",
            "nav.transfer": "Uhamisho", "nav.interbranch": "Uhamisho kati ya matawi", "nav.loans": "Mikopo", "nav.cards": "Kadi",
            "nav.administration": "USIMAMIZI", "nav.amlApprovals": "Idhini za AML", "nav.users": "Watumiaji",
            "nav.aml": "Tahadhari za AML", "nav.compensations": "Malipo ya matawi", "nav.cnrf": "CNRF", "nav.amlThresholds": "Viwango vya AML",
            "nav.settings": "Mipangilio", "nav.systemOperational": "Mfumo unafanya kazi", "nav.myAccount": "Akaunti yangu",
            "nav.logout": "Toka", "nav.login": "Ingia",
            "dashboard.eyebrow": "MUHTASARI", "dashboard.title": "Dashibodi",
            "dashboard.subtitle": "Fuatilia shughuli na takwimu muhimu za benki yako.", "dashboard.approvals": "Angalia uhamisho unaosubiri idhini",
            "dashboard.clients": "Jumla ya wateja", "dashboard.registeredClients": "Wateja waliosajiliwa",
            "dashboard.agencies": "Jumla ya matawi", "dashboard.network": "Matawi kwenye mtandao",
            "dashboard.deposits": "Amana za wateja", "dashboard.customerBalances": "Salio la akaunti za wateja kwa sarafu",
            "dashboard.toReview": "INASUBIRI", "dashboard.pendingTitle": "Uhamisho kati ya matawi",
            "dashboard.pendingDescription": "Kila uhamisho husubiri hadi msimamizi aukubali.",
            "dashboard.noPending": "Hakuna uhamisho wa kuidhinisha", "dashboard.allCaughtUp": "Foleni ya idhini imesasishwa.",
            "dashboard.pending": "Inasubiri", "dashboard.seeAll": "Fungua foleni ya idhini",
            "dashboard.shortcuts": "VIUNGO VYA HARAKA", "dashboard.quickActions": "Vitendo vya haraka",
            "dashboard.manageClients": "Simamia wateja", "dashboard.manageAgencies": "Simamia matawi",
            "dashboard.manageUsers": "Simamia watumiaji", "dashboard.approvalRule": "Udhibiti wa uhamisho",
            "dashboard.approvalRuleDescription": "Uhamisho kati ya matawi hauandikwi hadi msimamizi aukubali.",
            "settings.eyebrow": "MAPENDELEO YA PROGRAMU", "settings.title": "Mipangilio",
            "settings.subtitle": "Badilisha mwonekano na lugha ya eneo lako la kazi.",
            "settings.appearance": "Mwonekano", "settings.appearanceDescription": "Chagua mandhari ya programu.",
            "settings.darkMode": "Mandhari ya giza", "settings.darkModeDescription": "Hupunguza mwangaza wa kiolesura.",
            "settings.language": "Lugha", "settings.languageDescription": "Chagua lugha ya kiolesura.",
            "settings.interfaceLanguage": "Lugha ya programu", "settings.security": "Usalama na shughuli",
            "settings.securityDescription": "Sheria zinazotumika kwa shughuli za benki.",
            "settings.interbranchTransfers": "Uhamisho kati ya matawi",
            "settings.interbranchDescription": "Idhini ya msimamizi inahitajika kabla ya kuandika uhamisho.",
            "settings.mandatory": "Lazima", "settings.currency": "Sarafu kuu",
            "settings.currencyDescription": "Kiasi huonyeshwa kwa sarafu ya kila akaunti.",
            "settings.session": "Kipindi", "settings.sessionDescription": "Huisha baada ya dakika 60 bila shughuli.",
            "settings.savedLocally": "Mapendeleo yako ya mwonekano yamehifadhiwa kwenye kifaa hiki.",
            "title.Home.Index": "Dashibodi", "title.Home.Settings": "Mipangilio"
        }
    };

    const readPreference = (key, fallback) => {
        try { return localStorage.getItem(key) || fallback; }
        catch { return fallback; }
    };
    const writePreference = (key, value) => {
        try { localStorage.setItem(key, value); }
        catch { /* Keep the selected preference active for this page. */ }
    };

    let language = readPreference("akaranga.language", "fr");
    let theme = readPreference("akaranga.theme", "light");

    const renderPreferences = () => {
        const dictionary = translations[language] || translations.fr;
        document.documentElement.lang = language;
        document.documentElement.dataset.theme = theme;
        document.querySelectorAll("[data-i18n]").forEach(element => {
            const translation = dictionary[element.dataset.i18n];
            if (translation) element.textContent = translation;
        });
        document.querySelectorAll("[data-page-title]").forEach(element => {
            const translation = dictionary[`title.${element.dataset.pageTitle}`];
            if (translation) element.textContent = translation;
        });
        const languageSelect = document.getElementById("language-select");
        if (languageSelect) languageSelect.value = language;
    };

    window.akarangaPreferences = {
        getLanguage: () => language,
        setLanguage(value) {
            language = translations[value] ? value : "fr";
            writePreference("akaranga.language", language);
            renderPreferences();
        },
        setTheme(value) {
            theme = value === "dark" ? "dark" : "light";
            writePreference("akaranga.theme", theme);
            renderPreferences();
        }
    };

    renderPreferences();
})();
