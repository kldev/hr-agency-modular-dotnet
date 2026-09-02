using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine;

namespace HrAgencySystem.PlatformSeeder.Scenario;

internal sealed class ProductionJobDescriptionScenario(IMessageBus bus)
{
    public async Task Create(
        Guid organizationId,
        IReadOnlyList<Guid> userIds,
        IReadOnlyList<Guid> companyIds)
    {
        if (companyIds.Count == 0 || userIds.Count == 0)
            return;

        var companyIndex = 0;
        var userIndex = 0;

        await Create(
            userIds[++userIndex % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[++companyIndex % companyIds.Count],
                "Spawacz MAG",
                null,
                "Praca przy spawaniu konstrukcji stalowych metodą MAG. Oferta skierowana do osób z doświadczeniem w spawaniu elementów stalowych.",
                [
                    "Spawanie metodą MAG",
                    "Czytanie rysunku technicznego",
                    "Obsługa elektronarzędzi",
                    "Kontrola jakości wykonanych spoin",
                    "Przygotowanie elementów do spawania"
                ],
                [
                    "Doświadczenie w spawaniu metodą MAG",
                    "Umiejętność czytania rysunku technicznego",
                    "Aktualne uprawnienia spawalnicze będą dodatkowym atutem",
                    "Gotowość do pracy zmianowej",
                    "Dokładność i odpowiedzialność",
                    "Brak przeciwwskazań do pracy fizycznej"
                ],
                [
                    "Spawanie konstrukcji stalowych zgodnie z dokumentacją",
                    "Przygotowanie materiału do spawania",
                    "Wykonywanie spoin zgodnie z wymaganiami jakościowymi",
                    "Kontrola wykonanych połączeń",
                    "Dbanie o stanowisko pracy",
                    "Przestrzeganie zasad BHP"
                ],
                "Opole",
                "PL",
                EmploymentType.FullTime,
                WorkMode.OnSite,
                CurrencyCode.PLN,
                8500m,
                12000m,
                userIds[userIndex % userIds.Count], 
                userIds[userIndex % userIds.Count] 
            )
        );

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "Operator CNC",
                null,
                "Poszukujemy Operatora CNC do pracy przy produkcji elementów metalowych. Praca na nowoczesnych maszynach CNC.",
                [
                    "Obsługa maszyn CNC",
                    "Czytanie rysunku technicznego",
                    "Podstawy programowania CNC",
                    "Pomiar detali",
                    "Kontrola jakości"
                ],
                [
                    "Doświadczenie jako operator CNC",
                    "Umiejętność czytania rysunku technicznego",
                    "Znajomość podstaw obróbki skrawaniem",
                    "Umiejętność posługiwania się przyrządami pomiarowymi",
                    "Gotowość do pracy zmianowej",
                    "Dokładność"
                ],
                [
                    "Obsługa tokarek i frezarek CNC",
                    "Ustawianie parametrów maszyny",
                    "Kontrola wymiarów produkowanych elementów",
                    "Wprowadzanie korekt do programu",
                    "Przygotowanie materiału do produkcji",
                    "Prowadzenie podstawowej dokumentacji"
                ],
                "Nysa",
                "PL",
                EmploymentType.FullTime,
                WorkMode.OnSite,
                CurrencyCode.PLN,
                8000m,
                11500m,
                userIds[userIndex % userIds.Count],
                userIds[userIndex++ % userIds.Count]
            )
        );

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "Monter Konstrukcji Stalowych",
                null,
                "Praca przy montażu konstrukcji stalowych dla obiektów przemysłowych i magazynowych.",
                [
                    "Montaż konstrukcji stalowych",
                    "Czytanie rysunku technicznego",
                    "Obsługa elektronarzędzi",
                    "Pomiar i dopasowanie elementów",
                    "Prace ślusarskie"
                ],
                [
                    "Doświadczenie w montażu konstrukcji stalowych",
                    "Umiejętność czytania rysunku technicznego",
                    "Znajomość elektronarzędzi",
                    "Sprawność fizyczna",
                    "Gotowość do pracy na wysokości",
                    "Prawo jazdy będzie dodatkowym atutem"
                ],
                [
                    "Montaż elementów konstrukcji stalowych",
                    "Dopasowywanie elementów zgodnie z dokumentacją",
                    "Wiercenie i cięcie elementów",
                    "Wykonywanie prostych prac ślusarskich",
                    "Kontrola poprawności montażu",
                    "Przestrzeganie zasad bezpieczeństwa"
                ],
                "Kędzierzyn-Koźle",
                "PL",
                EmploymentType.FullTime,
                WorkMode.OnSite,
                CurrencyCode.PLN,
                7500m,
                11000m,
                userIds[userIndex % userIds.Count],
                userIds[userIndex % userIds.Count]
            )
        );

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "Pracownik Produkcji",
                null,
                "Oferta pracy na stanowisku Pracownika Produkcji w zakładzie produkcyjnym. Możliwość przyuczenia do pracy.",
                [
                    "Praca na linii produkcyjnej",
                    "Obsługa prostych maszyn",
                    "Kontrola wizualna produktów",
                    "Pakowanie produktów",
                    "Przestrzeganie instrukcji stanowiskowych"
                ],
                [
                    "Gotowość do pracy zmianowej",
                    "Sprawność manualna",
                    "Dokładność",
                    "Odpowiedzialność",
                    "Gotowość do pracy fizycznej",
                    "Doświadczenie produkcyjne będzie dodatkowym atutem"
                ],
                [
                    "Obsługa stanowiska produkcyjnego",
                    "Montaż i pakowanie produktów",
                    "Kontrola jakości wyrobów",
                    "Uzupełnianie materiałów produkcyjnych",
                    "Dbanie o porządek na stanowisku",
                    "Przestrzeganie zasad BHP"
                ],
                "Brzeg",
                "PL",
                EmploymentType.FullTime,
                WorkMode.OnSite,
                CurrencyCode.PLN,
                6500m,
                8500m,
                userIds[userIndex % userIds.Count],
                userIds[userIndex % userIds.Count]
            )
        );

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "Magazynier",
                null,
                "Poszukujemy Magazyniera do obsługi magazynu materiałów produkcyjnych i wyrobów gotowych.",
                [
                    "Obsługa wózka widłowego",
                    "Praca ze skanerem magazynowym",
                    "Kompletacja zamówień",
                    "Przyjmowanie dostaw",
                    "Kontrola stanów magazynowych"
                ],
                [
                    "Doświadczenie w pracy magazynowej",
                    "Uprawnienia UDT na wózki widłowe",
                    "Gotowość do pracy zmianowej",
                    "Umiejętność pracy ze skanerem",
                    "Dobra organizacja pracy",
                    "Odpowiedzialność"
                ],
                [
                    "Przyjmowanie i wydawanie towaru",
                    "Kompletowanie zamówień",
                    "Rozładunek dostaw",
                    "Przygotowywanie materiałów dla produkcji",
                    "Obsługa systemu magazynowego",
                    "Dbanie o porządek w magazynie"
                ],
                "Opole",
                "PL",
                EmploymentType.FullTime,
                WorkMode.OnSite,
                CurrencyCode.PLN,
                7000m,
                10000m,
                userIds[userIndex % userIds.Count],
                userIds[userIndex % userIds.Count]
            )
        );

        await Create(
            userIds[userIndex++ % userIds.Count],
            new CreateJobDescription(
                organizationId,
                companyIds[companyIndex++ % companyIds.Count],
                "Kontroler Jakości",
                null,
                "Stanowisko w dziale kontroli jakości odpowiedzialnym za kontrolę wyrobów produkcyjnych.",
                [
                    "Kontrola jakości",
                    "Czytanie rysunku technicznego",
                    "Pomiary przyrządami kontrolnymi",
                    "Dokumentacja jakościowa",
                    "Analiza niezgodności"
                ],
                [
                    "Doświadczenie w kontroli jakości",
                    "Umiejętność czytania rysunku technicznego",
                    "Znajomość przyrządów pomiarowych",
                    "Dokładność i skrupulatność",
                    "Umiejętność analizy problemów",
                    "Znajomość podstaw systemów jakości będzie atutem"
                ],
                [
                    "Kontrola jakości wyrobów",
                    "Wykonywanie pomiarów",
                    "Dokumentowanie wyników kontroli",
                    "Identyfikowanie niezgodności",
                    "Współpraca z działem produkcji",
                    "Udział w działaniach korygujących"
                ],
                "Krapkowice",
                "PL",
                EmploymentType.FullTime,
                WorkMode.OnSite,
                CurrencyCode.PLN,
                7500m,
                11000m,
                userIds[userIndex % userIds.Count],
                userIds[userIndex % userIds.Count]
            )
        );
    }

    private async Task Create(
        Guid userId,
        CreateJobDescription request)
    {
        await bus.InvokeAsync(
            request with
            {
                RecruiterId = userId
            });
    }
}
