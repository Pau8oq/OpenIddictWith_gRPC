using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translating.Web.Domain.Models
{
    public record class LanguageModel
    {
        public int Id { get; init; }
        public string Name { get; init; }
    }
}
