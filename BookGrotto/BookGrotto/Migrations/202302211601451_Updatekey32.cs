namespace BookGrotto.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Updatekey32 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.tb_OrderDetails");
            AddPrimaryKey("dbo.tb_OrderDetails", new[] { "OrderId", "ProductId" });
            DropColumn("dbo.tb_OrderDetails", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tb_OrderDetails", "Id", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.tb_OrderDetails");
            AddPrimaryKey("dbo.tb_OrderDetails", "Id");
        }
    }
}
