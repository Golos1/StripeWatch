## Purpose
StripeWatch is a background service that polls the Stripe API to check for events like a balance being too low or the presence of disputes.

## Required Arguments
1. --stripe-key-file is a path to a file containing a Stripe API secret key.
## Optional Arguments
1. --min is the minimum balance for an account, as a double.
2. --log-file is a filepath to log events to. NOTE: if using the docker image, a file must be mounted into the container as a volume at the same path.