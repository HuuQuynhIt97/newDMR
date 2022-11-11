using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class GlueIngredientForGroupByDto
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<GlueIngredientDetailDto> GlueIngredients { get; set; }

    }
}
