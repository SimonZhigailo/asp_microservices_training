using PlatformService.Model;

namespace PlatformService.Data
{
    public static class PrepDbo
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>());
            }    
        }

        private static void SeedData(AppDbContext context)
        {
            if (!context.Platforms.Any())
            {
                Console.WriteLine("Внедряю данные");

                context.Platforms.AddRange(
                    new Platform() {Name="DotNet", Publisher="Microsoft", Cost="Free"},
                    new Platform() {Name="Sql Server", Publisher="Microsoft", Cost="Free"},
                    new Platform() {Name="Kubernetes", Publisher="что-то там", Cost="Free"}
                
                );
                
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("Данные есть уже");
            }
        }
    }
}