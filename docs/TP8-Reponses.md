# Mini-TP 8 — Réponses de compréhension

## 1. Qu'est-ce que la partie double ? Pourquoi est-elle essentielle ?

Chaque opération est enregistrée au moins une fois au débit et une fois au crédit, pour le même montant. Le total des débits doit égaler le total des crédits. Cette règle garde les comptes cohérents et permet de repérer une écriture manquante ou erronée.

Dans ce projet, un dépôt crédite le compte du client et débite la caisse de l'agence. Un retrait débite le compte client et crédite la caisse. Les frais débitent le compte client et créditent le compte général des produits de frais. Un virement débite le compte source et crédite le compte destination.

## 2. Différence entre `Operation` et `Ecriture` ?

`Operation` décrit l'événement métier : type, référence, montant, date, compte concerné et utilisateur. `Ecriture` représente une ligne comptable de cet événement. Une opération peut donc produire plusieurs écritures, dont les débits et crédits doivent s'équilibrer.

## 3. Pourquoi une transaction EF Core est-elle nécessaire ?

Une transaction regroupe la création de l'opération, ses écritures et la mise à jour du solde. Si une étape échoue, tout est annulé. Cela évite d'avoir un solde modifié sans écritures correspondantes, ou des écritures sans opération valide.

## 4. Qu'est-ce que la LBC/FT ? Que fait la CNRF ?

La LBC/FT désigne la lutte contre le blanchiment de capitaux et le financement du terrorisme. Au Burundi, la CNRF est la Cellule Nationale de Renseignement Financier. Elle reçoit et analyse des informations financières et les diffuse aux autorités compétentes selon son mandat. Les seuils de cet exercice sont des paramètres applicatifs ; ils ne remplacent pas les obligations légales ou les procédures de conformité de la banque. Voir le [site officiel de la CNRF](https://cnrf.gov.bi/index.php/apercu/) et le [Ministère des Finances du Burundi](https://finances.gov.bi/index.php/2026/02/17/decret-no-100-009-du-09-02-2026-portant-modification-du-decret-no-100-044-du-16-03-2020-portant-creation-missions-organisation-et-fonctionnement-de-la-cellule-nationale-du-renseignement-financier-au/).

## 5. Différence entre vigilance renforcée et déclaration obligatoire ?

La vigilance renforcée signifie que l'établissement recueille davantage d'informations et surveille plus étroitement une relation ou une transaction. Une déclaration obligatoire est une transmission formelle aux autorités compétentes lorsque les critères légaux applicables sont réunis. Une alerte interne n'est pas, à elle seule, une déclaration officielle.

## 6. Qu'est-ce que le fractionnement ? Comment le détecter ?

Le fractionnement consiste à diviser une opération importante en plusieurs opérations plus petites, parfois pour éviter un seuil de contrôle. Le service additionne les dépôts récents sous le seuil sur une période de 24 heures et crée une alerte lorsque leur cumul atteint le seuil configuré. Un signal ne prouve pas une infraction : il appelle une analyse humaine.

## 7. Comment évaluer le risque d'un client ?

Évaluer le profil KYC, l'activité et l'origine des fonds, les produits utilisés, les zones géographiques pertinentes, les volumes et fréquences habituels, les écarts de comportement et les alertes antérieures. Le score du projet est pédagogique ; une décision réelle demande des règles validées, des données fiables et une revue par le personnel habilité.

## 8. Pourquoi ne jamais révéler au client qu'une alerte a été créée ?

Une alerte est un signal interne, pas une conclusion de culpabilité. La révéler peut compromettre une analyse ou une enquête et peut relever du « tipping-off », interdit par les règles applicables. Le personnel doit suivre la procédure de conformité et communiquer au client uniquement les informations autorisées.

## 9. Comment vérifier que la comptabilité est équilibrée ?

Additionner tous les débits et tous les crédits et vérifier leur égalité, puis contrôler chaque opération afin qu'une erreur sur une transaction ne soit pas masquée par une autre. `/Operations/ControleEquilibre` vérifie ici les totaux par opération et les schémas de comptabilisation définis ci-dessus.

## 10. Pourquoi `ApplicationUser` doit-il hériter d'`IdentityUser` ?

L'héritage permet à ASP.NET Core Identity de gérer l'identifiant, le nom de connexion, le mot de passe haché, les rôles, les claims, le verrouillage et les jetons. `ApplicationUser` ajoute les données propres à la banque sans réimplémenter ces fonctions de sécurité.
