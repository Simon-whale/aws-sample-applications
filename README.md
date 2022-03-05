# aws-sample-applications

This repo, is for reference when trying to interact with AWS, the whole project has been built around localstack so that you don't need to sign up to using AWS straightaway

# Installing Localstack on Docker

```
docker pull localstack/localstack
```

once the image has been downloaded you can run this by executing the following command

```
docker run -d --rm -it -p 4566:4566 localstack/localstack
```
