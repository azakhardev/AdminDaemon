# Backup Scheduler – Školní projekt pro zálohování dat

## Popis

Tento projekt vznikl jako týmová práce ve skupině tří lidí. Cílem bylo vytvořit funkční aplikaci pro **automatické zálohování dat podle zadaného rozvrhu a konfigurace**. Aplikace se skládala ze tří částí:

- **API** – zprostředkovávalo komunikaci mezi daemonem, administrátorským rozhraním a databází.
- **Daemon** – běžel na počítači, kontroloval naplánované zálohy a prováděl je podle nastavení (čas, typ, komprese atd.).
- **Administrátorské rozhraní** – webová aplikace v Angularu pro správu počítačů, konfigurací a přehledů.

Na začátku jsme navrhli **databázi** pomocí dbDiagram.io a rozhraní ve Figmě. Během vývoje jsem se podílel na všech částech aplikace, nejvíce na logice daemona a API. Webové rozhraní jsem vytvářel částečně ke konci projektu, kdy bylo potřeba pomoci s dokončením – i přesto, že jsem tehdy Typescript příliš neovládal.

Projekt běžel lokálně, nebyl určen pro veřejné nasazení. Práce na něm trvala přibližně dva a půl měsíce.

## Funkce

- Plánování a správa záloh přes webové rozhraní
- Podpora různých typů záloh a komprese
- Cron-like plánování
- Evidence více počítačů a uživatelů
- Šifrování vybraných dat

## Technologie

- **Backend**: C# (Console App & API)
- **Frontend**: Angular
- **Databáze**: SQL (návrh v dbDiagram.io)
- **Návrh UI**: Figma
