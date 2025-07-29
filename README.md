## Purpose
StripeWatch is a background service that polls the Stripe API to check for new events, and optionally to see if the balance is too low.
It sends these events as AMQP messages and optionally logs them to a file.
The messages will be sent to the queue 'stripe-events'.

## Required Arguments
1. --stripe-key is a Stripe API secret key.
2. --amqp is the url of a message broker to send to.
## Optional Arguments
1. --min is the minimum balance for an account, as a double.
2. --log-file is a filepath to log events to. NOTE: if using the docker image, a file must be mounted into the container as a volume at the same path.
3. --minutes (default 30) is the number of minutes after which to poll the Stripe API for events.The daemon will fail if the argument cannot be parsed as a positive integer.

## Getting Started
While the binaries here can be used, I recommend using the docker image for an easier and more reproducible experience.