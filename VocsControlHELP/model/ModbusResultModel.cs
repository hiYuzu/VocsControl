namespace VocsControlHELP.model
{
    public class ModbusResultModel
    {
        public string Time { set; get; }
        public short StateId { set; get; }
        public float Zt { set; get; }
        public float Jw { set; get; }

        public ModbusResultModel(string time, short stateId, float zt, float jw) {
            this.Time = time;
            this.StateId = stateId;
            this.Zt = zt;
            this.Jw = jw;
        }
    }
}
