namespace Pazar.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_image_path_column : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AdModels", "ImagePath", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AdModels", "ImagePath");
        }
    }
}
