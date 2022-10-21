namespace CovidLetter.Frontend.WebApp.Services.Queue
{
    public class UnattendedPdfRequest
    {
        public UnattendedPdfRequest(string fHIRPatient, string emailToSendTo,string mobileNumber, string correlationId)
        {
            FHIRPatient = fHIRPatient;
            EmailToSendTo = emailToSendTo;
            CorrelationId = correlationId;
            MobileNumber = mobileNumber;
        }

        public string FHIRPatient { get; set; }
        public string EmailToSendTo { get; set; } //Also Email to send faliure message   
        public string CorrelationId { get; set; }

        //Faliure Notification
        public ContactMethodEnum ContactMethodSettable { get; set; }

        public string MobileNumber { get; set; }
    }

}
