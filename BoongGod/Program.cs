using Redbean;

await Bootstrap.Setup(AppDomain.CurrentDomain.GetAssemblies());

var builder = WebApplication.CreateBuilder(args);
await AppProgram.Build(builder);
await AppProgram.Run(builder);