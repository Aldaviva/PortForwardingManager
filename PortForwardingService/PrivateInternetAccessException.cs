#nullable enable

using System;

namespace PortForwardingService; 

public class PrivateInternetAccessException: Exception {

    private PrivateInternetAccessException() { }

    public class UnknownForwardedPort: PrivateInternetAccessException { }

    public class PortForwardingDisabled: PrivateInternetAccessException { }

}