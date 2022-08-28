import fs from 'fs';
import path from 'path';
import convert from 'xml-js';
import { Feed } from 'feed';

// Based on: https://sreetamdas.com/blog/rss-for-nextjs

const feedsDirectory = path.join(process.cwd(), 'public/blog/feeds');
const feedTitle = 'Fluent Feeds Blog';
const feedDescription = 'A blog about the Fluent Feeds app.';
const feedCopyright = '© 2022, the Fluent Feeds developers'
const baseUrl = 'https://hannesschulze.github.io/fluent-feeds';
const blogUrl = `${baseUrl}/blog`;
const feedsUrl = `${blogUrl}/feeds`;
const feedNames = {
  rss2: 'main.xml',
  json: 'main.json',
  atom: 'atom.xml'
};

function generateAtomFeed(posts) {
  return convert.js2xml({
    _declaration: {
      _attributes: {
        version: '1.0',
        encoding: 'utf-8'
      }
    },
    feed: {
      _attributes: {
        xmlns: 'http://www.w3.org/2005/Atom'
      },
      title: {
        _text: feedTitle
      },
      subtitle: {
        _text: feedDescription
      },
      link: [
        {
          _attributes: {
            rel: 'alternate',
            type: 'text/html',
            href: blogUrl
          }
        },
        {
          _attributes: {
            rel: 'self',
            type: 'application/atom+xml',
            href: `${feedsUrl}/${feedNames.atom}`
          }
        }
      ],
      id: {
        _text: blogUrl
      },
      updated: {
        _text: new Date().toISOString()
      },
      rights: {
        _text: feedCopyright
      },
      entry: posts.map(post => {
        const postUrl = `${blogUrl}/posts/${post.slug}`;
        const postTimestamp = new Date(post.timestamp);
        return {
          title: {
            _cdata: post.title
          },
          link: {
            _attributes: {
              rel: 'alternate',
              type: 'text/html',
              href: postUrl
            }
          },
          id: {
            _text: postUrl
          },
          published: {
            _text: postTimestamp.toISOString()
          },
          updated: {
            _text: postTimestamp.toISOString()
          },
          author: {
            name: {
              _text: post.author
            }
          },
          summary: {
            _attributes: {
              type: 'text'
            },
            _cdata: post.summary
          },
          content: {
            _attributes: {
              type: 'html',
              'xml:base': postUrl,
              'xml:lang': 'en'
            },
            _cdata: post.content
          }
        };
      })
    }
  }, {
    compact: true,
    ignoreComment: true,
    spaces: 2
  });
}

export async function generateFeeds(posts) {
  const feed = new Feed({
    title: feedTitle,
    description: feedDescription,
    id: blogUrl,
    link: blogUrl,
    language: 'en-US',
    favicon: `${baseUrl}/favicon.ico`,
    updated: new Date(),
    copyright: feedCopyright,
    feedLinks: {
      rss2: `${feedsUrl}/${feedNames.rss2}`,
      json: `${feedsUrl}/${feedNames.json}`,
      atom: `${feedsUrl}/${feedNames.atom}`
    },
    author: { name: 'Fluent Feeds' }
  });

  posts.forEach(post => {
    const url = `${blogUrl}/posts/${post.slug}`;
    feed.addItem({
      title: post.title,
      id: url,
      link: url,
      description: post.summary,
      content: post.content,
      author: [{ name: post.author }],
      date: new Date(post.timestamp)
    });
  });

  await fs.promises.mkdir(feedsDirectory, { recursive: true });
  await fs.promises.writeFile(path.join(feedsDirectory, feedNames.rss2), feed.rss2());
  await fs.promises.writeFile(path.join(feedsDirectory, feedNames.json), feed.json1());
  await fs.promises.writeFile(path.join(feedsDirectory, feedNames.atom), generateAtomFeed(posts));
}
