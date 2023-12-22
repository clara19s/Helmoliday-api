# HELMoliday Web

Projet du cours d'architectures logicielles et du cours de frameworks web (année académique 2023-2024) réalisé par Clara SCHILTZ et Lionel BOVY.

Lien vers le projet en production : [https://porthos-intra.cg.helmo.be/Q210266](https://porthos-intra.cg.helmo.be/Q210266)
Lien vers la documentation Swagger : [https://porthos-intra.cg.helmo.be/Q210266/swagger/index.html](https://porthos-intra.cg.helmo.be/Q210266/swagger/index.html)

**⚠️ Activer la connexion VPN de HELMo**

## Utilisateurs
| Adresse e-mail         | Mot de passe |
|------------------------|--------------|
| claraschiltz@gmail.com | Hello@1234   |
| lionel@bovy.dev        | Hello&123    |

## Flow de connexion
1. L'utilisateur se connecte à l'application avec son adresse e-mail et son mot de passe via l'endpoint `/auth/login`
```json
{
  "email": "claraschiltz@gmail.com",
  "password": "Hello@1234"
}
```
2. L'utilisateur reçoit ses informations utilisateur
```json
{
  "id": "F4757319-11FC-4226-95CD-08DBD9568709",
  "email": "",
  "firstName": "Clara",
  "lastName": "Schiltz",
  "token": "..."
}
```
3. L'utilisateur peut émettre des requêtes sur des endpoints protégés en ajoutant l'en-tête Authorization dans ses requêtes accompagné du `Bearer Token` JWT.

## Problème connu
### Ajout d'une activité
Lors de l'ajout d'une activité, il se peut que l'API ne réponde plus à cause de l'envoi de mails répétés. Ce problème survient lorsqu'on ajoute plusieurs activités dans période de temps relativement courte.