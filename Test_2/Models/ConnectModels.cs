namespace Test_2.Models
        {

        using System;
        using System.ComponentModel.DataAnnotations;
        using System.ComponentModel.DataAnnotations.Schema;


        public class ConnectModels:IModel
            {
                public Guid Id { get; set; }
                public System.Guid UserId { get; set; }
                [ForeignKey("UserId")]
                public virtual TempUser TempModel { get; set; }
               
                public string Ip { get; set; }
                public int Port { get; set; }
                public string Login { get; set; }
                public string Key { get; set; }
            }
}