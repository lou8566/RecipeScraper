namespace Scraper.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Ingredients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecipeHeaderId = c.Int(nullable: false),
                        Ingredient = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RecipeHeaders", t => t.RecipeHeaderId, cascadeDelete: true)
                .Index(t => t.RecipeHeaderId);
            
            CreateTable(
                "dbo.RecipeHeaders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SurrogateId = c.Int(nullable: false),
                        Type = c.String(maxLength: 50),
                        ImageUri = c.String(maxLength: 255),
                        RecipeUri = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.SurrogateId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Ingredients", "RecipeHeaderId", "dbo.RecipeHeaders");
            DropIndex("dbo.RecipeHeaders", new[] { "SurrogateId" });
            DropIndex("dbo.Ingredients", new[] { "RecipeHeaderId" });
            DropTable("dbo.RecipeHeaders");
            DropTable("dbo.Ingredients");
        }
    }
}
