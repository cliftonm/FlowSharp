#pylint: disable=C0111, C0301, C0303, W0311, W0614, W0401, W0232, W0702, W0703, W0201
# Make sure pip and setup tools are up to date:
# python -m pip install -U pip
# pip install -U setuptools
# pip install requests
# pip install lxml
# pip install cssselect
import requests
import sys
from lxml import html
from lxml.cssselect import CSSSelector
from lxml import etree
 
url = "http://www.onthisday.com/birthdays/" + sys.argv[1] + "/" + sys.argv[2]
# print("URL: " + url)
page = requests.get(url)
tree = html.fromstring(page.content)
sel = CSSSelector('.section--person-of-interest')
pois = sel(tree)
 
for poi in pois:
 print(poi.xpath("div/div/div[1]/p")[0].text_content().encode('cp437', errors='replace'))
 
 