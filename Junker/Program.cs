// See https://aka.ms/new-console-template for more information

using Bogus;
using PZ17.Models;
using Database = PZ17.Database;

await using var db = new Database();

var genders = await db.GetAsync<Gender>().ToListAsync();

// var fakeClients = new Faker<Client>()
//     .StrictMode(true)
//     .RuleFor(c => c.LastName, f => f.Name.LastName())
//     .RuleFor(c => c.FirstName, f => f.Name.FirstName())
//     .RuleFor(c => c.GenderId, f => f.PickRandom(genders).GenderId)
//     .RuleFor(c => c.ClientId, f => f.Random.Int())
//     .RuleFor(c => c.Gender, f => null);
// fakeClients.Locale = "ru";
//
// var generated = fakeClients.Generate(100);
// foreach (var client in generated.Where(client => client is not null)) {
//     await db.InsertAsync(client);
// }

var fakeProcedures = new Faker<Procedure>()
    .StrictMode(true)
    .RuleFor(it => it.ProcedureId, f => 0)
    .RuleFor(it => it.ProcedureName, f => f.Commerce.Product())
    .RuleFor(it => it.BasePrice, f => f.Random.Decimal());
fakeProcedures.Locale = "ru";

var procedures = fakeProcedures.Generate(100);
foreach (var procedure in procedures.Where(it => it is not null)) {
    await db.InsertAsync(procedure);
}