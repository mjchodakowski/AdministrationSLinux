using System;
using System.ComponentModel.DataAnnotations;

namespace Test_2.Models
{
    public interface IModel
    {
            [Key]
            Guid Id { get; set; }
    }
}