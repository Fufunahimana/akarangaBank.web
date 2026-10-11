# Chapitre 12 — Prêts et cartes bancaires

## Prêts

- `/Prets` affiche les demandes. Un client ne voit que celles liées à son profil.
- `/Prets/Create` est accessible aux guichetiers, gestionnaires et administrateurs. Le compte choisi doit appartenir au client sélectionné et être actif.
- Les types de prêts `PERSONNEL`, `HABITAT` et `URGENCE` sont initialisés par la migration et contiennent leurs fourchettes de montant, durée et taux.
- Un gestionnaire ou administrateur peut valider une demande avec un taux dans la plage du produit, ou la rejeter avec un motif.
- Un administrateur débloque un prêt validé. Le compte est crédité, l'opération `DeblocagePret` reçoit des écritures débit/crédit équilibrées et l'échéancier est créé.
- Le paiement d'une échéance débite le compte. Une échéance en retard ajoute une pénalité de 0,5 % par mois commencé, avec un minimum de 500 BIF. Trois retards font passer le prêt au statut `Impaye`.
- Un administrateur peut lancer la mise à jour des retards depuis la liste des prêts.

## Cartes

- `/Cartes` affiche les cartes ; les clients ne voient que leurs propres cartes.
- Un administrateur émet une carte Visa, MasterCard ou Locale pour un compte actif, avec trois plafonds journaliers.
- Visa commence par `4`, MasterCard par `5` et Locale par `6`. Le numéro est masqué dans les pages.
- Une opposition avec motif `Perte`, `Vol`, `Fraude` ou `Autre` met la carte au statut correspondant. Le renouvellement expire l'ancienne carte et recopie ses plafonds sur la nouvelle.
- Les transactions vérifient le statut, l'expiration, le solde, les plafonds journaliers et le contrôle AML. Les autorisations créent une opération avec deux écritures équilibrées ; les refus sont conservés dans l'historique avec leur motif.

## Vérification et migration

Les tests sont dans [Chapter12LoanCardTests.cs](../../AkarangaBank.Tests/UnitTests/Services/Chapter12LoanCardTests.cs) et [Chapter12AuthorizationTests.cs](../../AkarangaBank.Tests/UnitTests/Services/Chapter12AuthorizationTests.cs). Pour les lancer depuis le dossier `AkarangaBank.Web` :

```powershell
dotnet test ..\AkarangaBank.Tests\AkarangaBank.Tests.csproj -c Release
```

La migration `20261008180200_Chapitre12PretsEtCartes` crée le catalogue de prêts et l'historique des transactions carte, puis complète les tables existantes. Elle n'est pas appliquée automatiquement. Après sauvegarde de la base et vérification de la chaîne de connexion, appliquez-la depuis `AkarangaBank.Web` avec :

```powershell
dotnet ef database update
```
