namespace PortForwardingService.PrivateInternetAccess;

public class PrivateInternetAccessException: Exception {

    private PrivateInternetAccessException() {}

    public class UnknownForwardedPort: PrivateInternetAccessException;

    public class PortForwardingDisabled: PrivateInternetAccessException;

    public class PortForwardingFailed: PrivateInternetAccessException;

}