using dmr_api.Models;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class BottomFactoryDto
    {

    }
    public class BottomFactoryForPrintParams
    {
        public List<int> Subpackages { get; set; }

    }
    public class DispatchParamsDto
    {
        public int MixingInfoID { get; set; }
        public int GlueNameID { get; set; }
        public int BuildingID { get; set; }
    }
    public class LineForReturnDto
    {
        public string Name { get; set; }
        public double AmountTotal { get; set; }
    }
    public class BottomFactoryForDispatchDto
    {
        public List<Dispatch> Dispatches { get; set; }
        public List<LineForReturnDto> Lines { get; set; }

    }
    public class BottomFactoryForReturnDto
    {
        public BottomFactoryForReturnDto()
        {
        }

        public BottomFactoryForReturnDto(Ingredient ingredient, List<Subpackage> subpackages, int mixingInfoID)
        {
            Ingredient = ingredient;
            Subpackages = subpackages;
            MixingInfoID = mixingInfoID;
        }

        public Ingredient Ingredient { get; set; }
        public int MixingInfoID  { get; set; }
        public List<Subpackage> Subpackages { get; set; }
        public string Code { get; set; }
        public string Batch { get; set; }
        
        
    }
    public class AddDispatchParams
    {
        public int MixingInfoID { get; set; }
        public int GlueNameID { get; set; }
        public string LineName { get; set; }
        public int BuildingID { get; set; }
        public string Option { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
    }
    public class UpdateDispatchParams
    {
        public int ID { get; set; }
        public double RemaningAmount { get; set; }
    }
    public class GenerateSubpackageParams
    {
        public int MixingInfoID { get; set; }
        public string GlueName { get; set; }
        public int BuildingID { get; set; }
        public int GlueNameID { get; set; }
        public double AmountOfChemical { get; set; }
        public int Can { get; set; }
    }
    public class ScanQRCodeParams
    {
        public string PartNO { get; set; }
        public string BatchNO { get; set; }
        public int GlueID { get; set; }
        public int MixingInfoID { get; set; }
        public string GlueName { get; set; }
        public int GlueNameID { get; set; }
        public int BuildingID { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
    }

}
