# Fountain

A simple, markdown driven blogging website.

## Project Goals

I love writting blogs in markdown. But sometimes I want the ability to post a new blog post without having to commit a new file, create a PR, babysit a CI job, etc.

So Fountain was created to allow the best of both worlds:

* Create new posts with markdown
* Launch new posts without a commit -> redeploy cycle

## Project State

This project is currently a POC. Lots of things to do before it's usable as intended:

* Introduce more front matter to blog post
* Implement a proper DB layer.
* Implement auth to allow people to edit
* Figure out how to generate the navbar
* Figure out how to properly generate the footer
* Implement some sort of testing framework
* Figure out how to deploy
* Figure out how upgrading versions might work?

## Known Issues

* When trying to nest pages (Attempted page route is `test/tester`), something isn't working correctly.
