## setup

from PIL import Image, ImageDraw, ImageFont
from subprocess import check_output


def get_concat_h_multi_resize(im_list, resample=Image.BICUBIC):
    min_height = min(im.height for im in im_list)
    im_list_resize = [im.resize((int(im.width * min_height / im.height), min_height), resample=resample)
                      for im in im_list]
    total_width = sum(im.width for im in im_list_resize)
    dst = Image.new("RGB", (total_width, min_height))
    pos_x = 0
    for im in im_list_resize:
        dst.paste(im, (pos_x, 0))
        pos_x += im.width
    return dst


def get_concat_v_multi_resize(im_list, resample=Image.BICUBIC):
    min_width = min(im.width for im in im_list)
    im_list_resize = [im.resize((min_width, int(im.height * min_width / im.width)), resample=resample)
                      for im in im_list]
    total_height = sum(im.height for im in im_list_resize)
    dst = Image.new("RGB", (min_width, total_height))
    pos_y = 0
    for im in im_list_resize:
        dst.paste(im, (0, pos_y))
        pos_y += im.height
    return dst


def get_concat_tile_resize(im_list_2d, resample=Image.BICUBIC):
    im_list_v = [get_concat_h_multi_resize(im_list_h, resample=resample) for im_list_h in im_list_2d]
    return get_concat_v_multi_resize(im_list_v, resample=resample)


def MergeImages(observer, optionsNumber):
    print("MergeImages(observer, optionsNumber)")
    filenameSummary = observer.pathResults + "/" + observer.levelStartTime + ".summary." + str(optionsNumber) + ".png"
    print(filenameSummary)

    images = [observer.pathResults + observer.levelStartTime + ".behavioural_performance." + str(optionsNumber) + ".png",
              observer.pathResults + observer.levelStartTime + ".z.attention." + str(optionsNumber) + ".png",
              observer.pathResults + observer.levelStartTime + ".z.test." + str(optionsNumber) + ".png",
              observer.pathResults + observer.levelStartTime + ".y.attention." + str(optionsNumber) + ".png",
              observer.levelFolder + observer.levelStartTime + ".HeadsetControllerEyeFirstPerson." + str(optionsNumber) + ".png",
              observer.levelFolder + observer.levelStartTime + ".Headset." + str(optionsNumber) + ".png",
              observer.pathResults + observer.levelStartTime + ".y.test." + str(optionsNumber) + ".png",
              observer.levelFolder + observer.levelStartTime + ".Controller." + str(optionsNumber) + ".png",
              observer.levelFolder + observer.levelStartTime + ".Eye." + str(optionsNumber) + ".png"]

    im = []

    for i in images:
        im.append(Image.open(i))

    im2 = [[im[0], im[1], im[2]], [im[3], im[4], im[5]], [im[6], im[7], im[8]]]
    get_concat_tile_resize(im2).save(filenameSummary)

    image = Image.open(filenameSummary)

    draw = ImageDraw.Draw(image)
    font = ImageFont.truetype("arial.ttf", 60, encoding="unic")
    draw.line((3000*1/2, 1/3*3000, 3000*1/2, 3000), fill=(255, 255, 255, 255), width=5)
    draw.line((3000*1/2+1000, 1/3*3000, 3000*1/2+1000, 3000), fill=(255, 255, 255, 255), width=5)
    draw.line((1/3*3000, 3000*1/2+1000, 3000, 3000*1/2+1000), fill=(255, 255, 255, 255), width=5)
    draw.line((1/3*3000, 3000*1/2+1000, 3000, 3000*1/2+1000), fill=(255, 255, 255, 255), width=5)
    draw.line((1/3*3000, 1500, 3000, 1500), fill=(255, 255, 255, 255), width=5)

    font = ImageFont.truetype("arial.ttf", 60, encoding="unic")
    draw.text((1050, 1050), "All Sources", fill="#FFFFFF", font=font)
    draw.text((2050, 1050), "Headset", fill="#FFFFFF", font=font)
    draw.text((1050, 2050), "Controller", fill="#FFFFFF", font=font)
    draw.text((2050, 2050), "Gaze", fill="#FFFFFF", font=font)

    image.save(filenameSummary)
    # check_output("start " + filenameSummary2, shell=True)  # view