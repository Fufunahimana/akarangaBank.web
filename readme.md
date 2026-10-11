# Projet ASP.NET - UPG

Ce dépôt contient le projet pratique réalisé dans le cadre du cours d'ASP.NET enseigné à l'**Université Polytechnique de Gitega (UPG)**, au sein de la faculté **TIC**, département de **Génie Logiciel (Bac III)**.

## 👥 Auteurs
Ce projet a été conçu et développé en binôme par :
* **Nahimana Fulgence**
* **Niyorukundo Noella**

---

## 📌 À propos du projet
L'objectif principal de ce projet est de mettre en pratique les concepts fondamentaux et avancés du développement web avec le framework ASP.NET vus au cours. Il permet d'illustrer la maîtrise de la création d'applications web robustes, sécurisées et structurées selon les bonnes pratiques de l'architecture logicielle.

## 🛠️ Technologies utilisées
* **Langage :** C#
* **Framework :** ASP.NET (Core / MVC selon l'implémentation)
* **Base de données :** SQL Server (ou autre SGBD configuré)
* **Outils :** Visual Studio / .NET CLI

## 🚀 Fonctionnalités principales
* Gestion des utilisateurs / Authentification (le cas échéant)
* Opérations CRUD complètes (Création, Lecture, Mise à jour, Suppression)
* Interface utilisateur intuitive et responsive
* Connexion à la base de données via Entity Framework (ou approche similaire)

## ⚙️ Installation et exécution
Pour lancer ce projet en local, suivez les étapes ci-dessous :

1. Clonez ce dépôt sur votre machine :
   ```bash
   git clone https://github.com/Fufunahimana/akarangaBank.git
   ```
2. Ouvrez le projet dans **Visual Studio** ou votre éditeur de code favori.
3. Restaurez les packages NuGet et configurez la chaîne de connexion à votre base de données dans le fichier `appsettings.json`.
4. Exécutez les migrations de la base de données si nécessaire :
   ```bash
   dotnet ef database update
   ```
5. Lancez l'application en appuyant sur `F5` ou via la commande :
   ```bash
   dotnet run
   ```

---
*Université Polytechnique de Gitega (UPG) — Faculté TIC — Génie Logiciel Bac III (2025-2026)*

## Déploiement sur Railway

Ajoutez un service MySQL au projet Railway, puis liez ses variables au service web dans **Variables**. Créez les références suivantes en remplaçant `MySQL` par le nom exact de votre service de base de données :

```text
MYSQLHOST=${{MySQL.MYSQLHOST}}
MYSQLPORT=${{MySQL.MYSQLPORT}}
MYSQLDATABASE=${{MySQL.MYSQLDATABASE}}
MYSQLUSER=${{MySQL.MYSQLUSER}}
MYSQLPASSWORD=${{MySQL.MYSQLPASSWORD}}
```

L'application construit la chaîne de connexion à partir de ces variables si `ConnectionStrings__AkarangaBank` n'est pas défini. Vous pouvez aussi définir directement cette dernière variable avec une chaîne MySQL valide. Ne stockez pas les identifiants de la base dans les fichiers du dépôt.
