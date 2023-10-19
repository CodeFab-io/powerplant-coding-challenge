# powerplant-coding-challenge

## A potential solution, by Fábio Beirão

### High-level overview

There are 3 dotnet projects: `powerplant-coding-challenge`, `REST_API`, `Tests`

`powerplant-coding-challenge` is where are the models and calculations exist.

`REST_API` only concerns itself with API matters: serialization, deserialization and invoking the `powerplant-coding-challenge` function.

`Tests` is primarily focused on testing the internals of `powerplant-coding-challenge`, according to the requirements and provided examples.

## Code style

For this assignment I used my preferred style of writing c# code (and nearly any type of production code I write these days).

Depending on how familiar you are with functional languages, the approach might either look alien or familiar.

For me, the great advantage of functional-style code is the restrictions on what the code **cannot** do: functions are not allowed to "reach out" to the universe/context,
or mutate any of the arguments. Another way to look at it is that a (pure) function will always return the same output, given the same inputs.

I would be happy to discuss further about the different trade-offs of functional-style programming.

You will also see things such as `public sealed record Load(decimal LoadValue)`. This is a pattern I adopted to move away from primitive obsession. To me this solves different problems:

* Battle scars from mixing different compatible types (strings with strings, decimals with decimals, etc) and the headache of debugging such silly mistakes;

* Increases the clarity (from my point of view) for the consumer of the functions: they don't just have to provide a `decimal` as input to a function, they need to provide a `Load`;

* You get to better leverage the compiler when you have to perform refactors. I am usually a fan of trying to refactor something and then following the compiler and unit tests to get everything working again.

## Unit tests

In order to run the unit tests you can simply run `dotnet test` (on the root of the project) or use your favorite IDE (tested on Visual Studio 2022 Community Edition).


## Docker

The requirement states that we should export port `8888` so we can achieve this using the Docker port mapping (mapping the container's internal port `80` to the host's port `8888`).

Second step is building the container. On the root of the project we can run:

```
docker build -t powerplant -f .\REST_API\Dockerfile .
```

This will use the `Dockerfile` in `REST_API\Dockerfile` to build an image called `powerplant`.

Next we can run this image:

```
docker run --rm -p 8888:80 powerplant
```

(the parameter `--rm` makes Docker auto destroy the container once it exits, to prevent clutter on your machine).

If all goes well, we should see some logs in the console.

We can now open the browser in `http://localhost:8888/swagger` to use the SwaggerUI and interact with the API.

## Cleanup

We executed `docker run` with the `--rm` flag, so that cleared the container, but we still have the `powerplant` image lying around.

To clean up we should run `docker rmi powerplant`

## Added details

I will be honest that I have some unfamiliarity with the Energy Management sector. I understand, from my own curiosity, and from following youtube
creators such as Pratical Engineering, that there is both an inherent complexity as well as a built-in resiliency on the power grid. I have always
found it amazing the fact that the power grid needs to meet demand in real-time to keep the voltage on the network stable.

For this assignment, it wasn't stated (or clear to me) what should happen in scenarios where the Load is higher than all powerplants are able to meet,
and vice-versa what happens when all plants have a higher pmin than the Load.

For this reason, I decided to add an additional header `unsatisfied-load` on the response. Under normal circumstances, this will be `0`. However,
when the powerplants were not able to meet demand, this value would be a positive value of unmet KWh. On the flip-side, if due to pmin we exceeded the
load, this value will be negative, signaling (perhaps) that energy storage should be activated.

## Final remarks

All in all this was a fun assignment, I am always thrilled to explore new domains and concepts. I am looking forward to hearing your feedback,
because feedback is how I keep my skills sharp. 
Thank you for your time,

Fábio Beirão
fabio@codefab.io