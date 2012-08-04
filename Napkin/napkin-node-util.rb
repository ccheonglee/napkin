require 'yaml'
require 'rubygems'
require 'neo4j'

#
require 'napkin-config'

module Napkin
  module NodeUtil
    #
    #
    class NodeNav
      attr_accessor :node
      def initialize(node = Neo4j.ref_node)
        @node = node
      end

      def reset(node = Neo4j.ref_node)
        @node = node
      end

      def [](key)
        return @node[key]
      end

      def []=(key, value)
        Neo4j::Transaction.run do
          @node[key] = value
        end
      end

      def init_property(key, default)
        property_initialized = true
        Neo4j::Transaction.run do
          if (@node[key].nil?) then
            @node[key] = default
          else
            property_initialized = false
          end
        end
        return property_initialized
      end

      def get_or_init(key, default)
        value = @node[key]
        if (value.nil?) then
          Neo4j::Transaction.run do
            value = @node[key]
            if (value.nil?) then
              @node[key] = default
              value = default
            end
          end
        end
        return value
      end

      def go_sub(id)
        sub = @node.outgoing(:sub).find{|sub| sub[:id] == id}
        if (sub.nil?) then
          return false
        else
          @node = sub
          return true
        end
      end

      def go_sub!(id)
        created_node = false
        if (!go_sub(id)) then
          Neo4j::Transaction.run do
            sub = @node.outgoing(:sub).find{|sub| sub[:id] == id}
            if (sub.nil?) then
              sub = Neo4j::Node.new :id => id
              @node.outgoing(:sub) << sub
              created_node = true
            end
            @node = sub
          end
        end
        return created_node;
      end

      def go_sub_path(path)
        path_segments = path.split('/')

        missed_count = path_segments.length
        path_segments.each do |segment|
          if (go_sub(segment)) then
            missed_count -= 1
          else
            break;
          end
        end

        return missed_count
      end

      def go_sub_path!(path)
        path_segments = path.split('/')
        sub_exists = true

        created_count = 0
        path_segments.each do |segment|
          if(go_sub!(segment)) then
            created_count += 1
          end
        end

        return created_count
      end

      def go_sup
        sup = @node.incoming(:sub).first()
        if (sup.nil?) then
          return false
        else
          @node = sup
          return true
        end
      end

      def get_path
        nn = dup
        path = "#{nn.get_segment}"
        while (nn.go_sup())
          path = "#{nn.get_segment}/" + path
        end
        return path
      end

      def get_segment
        segment = @node[:id]
        segment.nil? ? 'nil' : segment
      end
    end

    #
    #
    class PropertyMapper
      def initialize(prefix, keys, separator = '.')
        @prefix = prefix
        @keys = keys
        @separator = separator
      end

      def yaml_to_hash(yaml_text)
        yaml = YAML.load(yaml_text)
        out = {}
        @keys.each do |key|
          val = yaml[key]
          if (!val.nil?) then
            out[key] = val
          end
        end
        return out
      end

      def hash_to_yaml(hash)
        return YAML.dump(hash)
      end

      def node_to_hash(node)
        hash = {}
        @keys.each do |key|
          hash[key] = node[prefix_key(key)]
        end
        return hash
      end

      def hash_to_node(node, hash)
        Neo4j::Transaction.run do
          @keys.each do |key|
            node[prefix_key(key)] = hash[key]
          end
        end
      end

      def prefix_key(key)
        "#{@prefix}#{@separator}#{key}"
      end
    end

    #    module Props
    #      FEED_PROPS = PropertyMapper.new(['name','url','refresh_enabled','refresh_in_minutes'])
    #
    #      FILE_META_PROPS = PropertyMapper.new(['etag', 'last-modified', 'date', 'expires'])
    #      CHANNEL_PROPS = PropertyMapper.new(['title', 'link', 'description', 'pubDate', 'lastBuildDate'])
    #      ITEM_PROPS = PropertyMapper.new(['title','link', 'description','guid','pubDate'])
    #    end

  end
end
