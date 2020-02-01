# Azure Functions - Image Thumbnail

An azure function to generate thumbnail of an image.

## Setup

This code is written for azure function using dot net core. It also contains a visual studio solution and project which can be used to publish/deploy on azure function.

## Trigger

Once the function is up and running. The function will be triggered using blob trigger.

## Configuration

In the Configuration > Application settings sections of your azure function, add appropriate configuration with following:


| Key                         | Value                                            |
| --------------------------- |:------------------------------------------------:|
| THUMBNAIL_WIDTH             | width of the thumbnail generated                 |
| MINITHUMBNAIL_WIDTH         | width of mini thumbnail generated                |
| THUMBNAIL_CONTAINER_NAME    | thumbnail's azure blob storage container name    |
| STORAGE_CONNECTION_STRING   | thumbnail's azure blob storage connection string |

