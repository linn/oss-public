package uk.co.linn.gwt.services.client;

public class ServiceException extends Exception {
	
    public ServiceException(int aCode, String aDescription) {
        super("Error " + String.valueOf(aCode) + ": " + aDescription);
        iCode = aCode;
        iDescription = aDescription;
    }
    
    public int Code() {
        return (iCode);
    }
    
    public String Description() {
        return (iDescription);
    }
    
    private int iCode;
    private String iDescription;
}


