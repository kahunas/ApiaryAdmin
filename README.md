# ApiaryAdmin

## Aprašymas
ApiaryAdmin sistema skirta bitininkams administruoti savo bitynus. Naudotojai gali kurti bitynus, pridėti avilius ir vykdyti jų apžiūras, taip pat planuoti darbus. Sistema padeda tvarkyti informaciją apie bitynų būklę, avilių būklę ir atliktas apžiūras.

Taikomosios srities objektai: **Bitynas** (Apiary) -> **Avilys** (Hive) -> **Apžiūra** (Inspection).

---
## Funkcionalumas (funkciniai reikalavimai)

* Sistemoje veiks naudotojų autentifikacija, o naudotojai bus skirstomi į registruotus naudotojus (bitininkus) ir administratorius.
* **Registruotas naudotojas** (bitininkas) gali atlikti šiuos veiksmus:
  - Kurti ir valdyti savo bitynus.
  - Pridėti naujus avilius į savo bitynus.
  - Registruoti avilių apžiūras ir tvarkyti apžiūrų įrašus.
  - Atlikti CRUD (sukurti, peržiūrėti, redaguoti, pašalinti) operacijas tik su savo turiniu (bitynais, aviliais, apžiūromis).
* **Administratorius** turi visas teises ir gali valdyti ne tik savo, bet ir kitų naudotojų turinį bei naudotojų paskyras.
* **Darbo planavimas**: Bitininkai gali planuoti darbus, susijusius su bitynų priežiūra ir avilių apžiūromis, nustatydami datas ir užduotis.
* **Paieška ir filtrai**: Naudotojai gali ieškoti bitynų ir avilių pagal pavadinimus, lokacijas arba specifinius kriterijus (pvz., avilio būklę).
* **Ataskaitos ir statistika**: Sistema leidžia generuoti ataskaitas apie bitynų ir avilių būklę, bei atliktų apžiūrų istoriją.

---
Naudojamos technologijos:
- **Frontend**: Kažkas
- **Backend**: .NET (ASP.NET Core)
- **Duomenų bazė**: PostgreSQL